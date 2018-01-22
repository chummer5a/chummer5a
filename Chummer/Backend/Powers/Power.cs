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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Skills;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

// ReSharper disable once CheckNamespace
namespace Chummer
{
    /// <summary>
    /// An Adept Power.
    /// </summary>
    public class Power : INotifyPropertyChanged, IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private Guid _sourceID = Guid.Empty;
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
            objWriter.WriteElementString("id", _sourceID.ToString("D"));
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
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

        public bool Create(XmlNode objNode, int intRating = 1, XmlNode objBonusNodeOverride = null, bool blnCreateImprovements = true)
        {
            Name = objNode["name"].InnerText;
            _sourceID = Guid.Parse(objNode["id"].InnerText);
            _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("points", ref _strPointsPerLevel);
            objNode.TryGetStringFieldQuickly("adeptway", ref _strAdeptWayDiscount);
            LevelsEnabled = objNode["levels"]?.InnerText == System.Boolean.TrueString;
            Rating = intRating;
            if (!objNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevel);
            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
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
            if (blnCreateImprovements && Bonus != null && Bonus.HasChildNodes)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = Extra;
                if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId, Bonus, false, TotalRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    ImprovementManager.ForcedValue = strOldForce;
                    this.Deleting = true;
                    CharacterObject.Powers.Remove(this);
                    OnPropertyChanged(nameof(TotalRating));
                    return false;
                }
                Extra = ImprovementManager.SelectedValue;
                ImprovementManager.SelectedValue = strOldSelected;
                ImprovementManager.ForcedValue = strOldForce;
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
            string strId = objNode["id"]?.InnerText;
            if (!string.IsNullOrEmpty(strId))
            {
                _sourceID = Guid.Parse(strId);
                _objCachedMyXmlNode = null;
            }
            else
            {
                string strPowerName = Name;
                if (strPowerName.Contains('('))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf('(') - 1);
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                if (objXmlPower != null)
                {
                    _sourceID = Guid.Parse(objXmlPower["id"].InnerText);
                    _objCachedMyXmlNode = null;
                }
            }
            Extra = objNode["extra"].InnerText ?? string.Empty;
            _strPointsPerLevel = objNode["pointsperlevel"]?.InnerText;
            objNode.TryGetField("action", out _strAction);
            _strAdeptWayDiscount = objNode["adeptway"]?.InnerText;
            if (string.IsNullOrEmpty(_strAdeptWayDiscount))
            {
                string strPowerName = Name;
                if (strPowerName.Contains('('))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf('(') - 1);
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                _strAdeptWayDiscount = objXmlPower?["adeptway"]?.InnerText ?? string.Empty;
            }
            Rating = Convert.ToInt32(objNode["rating"]?.InnerText);
            LevelsEnabled = objNode["levels"]?.InnerText == System.Boolean.TrueString;
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
                _nodAdeptWayRequirements = objNode["adeptwayrequires"] ?? GetNode()?["adeptwayrequires"];
            }
            if (Name != "Improved Reflexes" && Name.StartsWith("Improved Reflexes"))
            {
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"Improved Reflexes\")]");
                if (objXmlPower != null)
                {
                    if (int.TryParse(Name.TrimStart("Improved Reflexes", true).Trim(), out int intTemp))
                    {
                        Create(objXmlPower, intTemp, null, false);
                        objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
                    }
                }
            }
            else
            {
                XmlNodeList nodEnhancements = objNode["enhancements"]?.SelectNodes("enhancement");
                if (nodEnhancements != null)
                {
                    foreach (XmlNode nodEnhancement in nodEnhancements)
                    {
                        Enhancement objEnhancement = new Enhancement(CharacterObject);
                        objEnhancement.Load(nodEnhancement);
                        objEnhancement.Parent = this;
                        Enhancements.Add(objEnhancement);
                    }
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("pointsperlevel", PointsPerLevel.ToString(objCulture));
            objWriter.WriteElementString("adeptway", AdeptWayDiscount.ToString(objCulture));
            objWriter.WriteElementString("rating", LevelsEnabled ? TotalRating.ToString(objCulture) : "0");
            objWriter.WriteElementString("totalpoints", PowerPoints.ToString(objCulture));
            objWriter.WriteElementString("action", DisplayActionMethod(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Print(objWriter, strLanguageToPrint);
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
        public string InternalId => _guiID.ToString("D");

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
        public IList<Enhancement> Enhancements { get; } = new List<Enhancement>();

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = Name;

            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
            }

            return strReturn;
        }

        /// <summary>
        /// The translated name of the Power (Name + any Extra text).
        /// </summary>
        public string DisplayName => DisplayNameMethod(GlobalOptions.Language);

        /// <summary>
        /// The translated name of the Power (Name + any Extra text).
        /// </summary>
        public string DisplayNameMethod(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += " (" + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
        }

        /// <summary>
        /// Power Point cost per level of the Power.
        /// </summary>
        public decimal PointsPerLevel
        {
            get
            {
                decimal decReturn = Convert.ToDecimal(_strPointsPerLevel, GlobalOptions.InvariantCultureInfo);
                return decReturn;
            }
            set => _strPointsPerLevel = value.ToString(GlobalOptions.InvariantCultureInfo);
        }

        /// <summary>
        /// An additional cost on top of the power's PointsPerLevel. 
        /// Example: Improved Reflexes is properly speaking Rating + 0.5, but the math for that gets weird. 
        /// </summary>
        public decimal ExtraPointCost
        {
            get => _decExtraPointCost;
            set => _decExtraPointCost = value;
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
            set => _strAdeptWayDiscount = value.ToString(GlobalOptions.InvariantCultureInfo);
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
            get => Math.Min(Rating + FreeLevels, TotalMaximumLevels);
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
                int intReturn = CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra && objImprovement.Enabled).Sum(objImprovement => objImprovement.Rating);
                // The power has an extra cost, so free PP from things like Qi Foci have to be charged first. 
                if (Rating + intReturn == 0 && ExtraPointCost > 0)
                {
                    decExtraCost -= (PointsPerLevel + ExtraPointCost);
                    if (decExtraCost >= 0)
                    {
                        intReturn += 1;
                    }
                    for (decimal i = decExtraCost; i >= 1; --i)
                    {
                        intReturn += 1;
                    }
                }
                //Either the first level of the power has been paid for with PP, or the power doesn't have an extra cost.
                else
                {
                    for (decimal i = decExtraCost; i >= PointsPerLevel; i -= PointsPerLevel)
                    {
                        intReturn += 1;
                    }
                }
                int intMAG = CharacterObject.MAG.TotalValue;
                if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                    intMAG = CharacterObject.MAGAdept.TotalValue;
                return Math.Min(intReturn, intMAG);
            }
        }

        /// <summary>
        /// Total number of Power Points the Power costs.
        /// </summary>
        public decimal PowerPoints
        {
            get
            {
                if (_blnFree || Rating == 0 || !LevelsEnabled && FreeLevels > 0)
                {
                    return 0;
                }

                decimal decReturn = 0;
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
                return Math.Max(decReturn, 0.0m);
            }            
        }

        public string DisplayPoints
        {
            get
            {
                if (!string.IsNullOrEmpty(_displayPoints))
                    return _displayPoints;
                else
                    return PowerPoints.ToString("G29", GlobalOptions.CultureInfo);
            }
            set => _displayPoints = value;
        }

        /// <summary>
        /// Bonus source.
        /// </summary>
        public string BonusSource
        {
            get => _strBonusSource;
            set => _strBonusSource = value;
        }

        /// <summary>
        /// Free Power Points that apply to the Power. Calculated as Improvement Rating * 0.25.
        /// Typically used for Qi Foci. 
        /// </summary>
        public decimal FreePoints
        {
            get
            {
                int intRating = CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreePoints && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra && objImprovement.Enabled).Sum(objImprovement => objImprovement.Rating);
                return intRating * 0.25m;
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
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
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
            get => _intMaxLevel;
            set => _intMaxLevel = value;
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 50% from Adept Way.
        /// </summary>
        public bool DiscountedAdeptWay
        {
            get => _blnDiscountedAdeptWay;
            set
            {
                if (value == _blnDiscountedAdeptWay) return;
                _blnDiscountedAdeptWay = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 25% from Geas.
        /// </summary>
        public bool DiscountedGeas
        {
            get => _blnDiscountedGeas;
            set => _blnDiscountedGeas = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Action.
        /// </summary>
        public string Action
        {
            get => _strAction;
            set => _strAction = value;
        }

        /// <summary>
        /// Translated Action.
        /// </summary>
        public string DisplayAction => DisplayActionMethod(GlobalOptions.Language);

        /// <summary>
        /// Translated Action.
        /// </summary>
        public string DisplayActionMethod(string strLanguage)
        {
            string strReturn = string.Empty;

            switch (_strAction)
            {
                case "Auto":
                    strReturn = LanguageManager.GetString("String_ActionAutomatic", strLanguage);
                    break;
                case "Free":
                    strReturn = LanguageManager.GetString("String_ActionFree", strLanguage);
                    break;
                case "Simple":
                    strReturn = LanguageManager.GetString("String_ActionSimple", strLanguage);
                    break;
                case "Complex":
                    strReturn = LanguageManager.GetString("String_ActionComplex", strLanguage);
                    break;
                case "Interrupt":
                    strReturn = LanguageManager.GetString("String_ActionInterrupt", strLanguage);
                    break;
                case "Special":
                    strReturn = LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);
                    break;
            }

            return strReturn;
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
                    int intMAG = CharacterObject.MAG.TotalValue;
                    if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                        intMAG = CharacterObject.MAGAdept.TotalValue;
                    intReturn = Math.Max(intReturn, intMAG);
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
                    int intMAG = CharacterObject.MAG.TotalValue;
                    if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                        intMAG = CharacterObject.MAGAdept.TotalValue;
                    intReturn = Math.Min(intReturn, intMAG);
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
                if (AdeptWayDiscount == 0)
                {
                    return false;
                }
                bool blnReturn = false;
                //If the Adept Way Requirements node is missing OR the Adept Way Requirements node doesn't have magicianswayforbids, check for the magician's way discount. 
                if (_nodAdeptWayRequirements?["magicianswayforbids"] == null)
                {
                    blnReturn = CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.MagiciansWayDiscount && x.Enabled);
                }
                if (!blnReturn && _nodAdeptWayRequirements != null)
                {
                    foreach (XmlNode objNode in _nodAdeptWayRequirements.SelectNodes("required/oneof/quality"))
                    {
                        string strExtra = objNode.Attributes?["extra"]?.InnerText;
                        if (!string.IsNullOrEmpty(strExtra))
                        {
                            blnReturn = CharacterObject.Qualities.Any(objQuality => objQuality.Name == objNode.InnerText && objQuality.Extra == strExtra);
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
                if (!blnReturn && DiscountedAdeptWay)
                {
                    DiscountedAdeptWay = false;
                }
                return blnReturn;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
            if (propertyName == nameof(TotalRating) && Bonus?.InnerXml.Contains("Rating") == true)
            {
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId);
                if (!Deleting)
                {
                    ImprovementManager.ForcedValue = Extra;
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId, Bonus, false, TotalRating, DisplayNameShort(GlobalOptions.Language));
                }
            }
        }

        /// <summary>
        /// Is the currently power being deleted? 
        /// Ugly hack to prevent powers with Ratings recreating their improvments when they're being deleted. TODO: FIX THIS BETTER
        /// </summary>
        public bool Deleting { internal get; set; }

        public string Category { get; set; }

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("powers.xml", strLanguage)?.SelectSingleNode("/chummer/powers/power[id = \"" + _sourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// ToolTip that shows how the Power is calculating its Modified Rating.
        /// </summary>
        public string ToolTip()
        {
            StringBuilder strbldModifier = new StringBuilder("Rating (");
            strbldModifier.Append(Rating);
            strbldModifier.Append(" x ");
            strbldModifier.Append(PointsPerLevel);
            strbldModifier.Append(')');
            foreach (Improvement objImprovement in CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra && objImprovement.Enabled))
            {
                strbldModifier.Append(" + ");
                strbldModifier.Append(CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language));
                strbldModifier.Append(" (");
                strbldModifier.Append(objImprovement.Rating.ToString());
                strbldModifier.Append(')');
            }

            return strbldModifier.ToString();
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
