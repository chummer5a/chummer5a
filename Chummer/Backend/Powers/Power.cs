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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

// ReSharper disable once CheckNamespace
namespace Chummer
{
    /// <summary>
    /// An Adept Power.
    /// </summary>
    [DebuggerDisplay("{DisplayNameMethod(GlobalOptions.DefaultLanguage)}")]
    public class Power : INotifyMultiplePropertyChanged, IHasInternalId, IHasName, IHasXmlNode, IHasNotes
    {
        private Guid _guiID;
        private Guid _sourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strPointsPerLevel = "0";
        private string _strAction = string.Empty;
        private decimal _decExtraPointCost;
        private int _intMaxLevel;
        private bool _blnDiscountedAdeptWay;
        private bool _blnDiscountedGeas;
        private XmlNode _nodAdeptWayRequirements;
        private string _strNotes = string.Empty;
        private string _strAdeptWayDiscount = "0";
        private string _strBonusSource = string.Empty;
        private decimal _decFreePoints;
        private string _strCachedPowerPoints = string.Empty;
        private bool _blnLevelsEnabled;
        private int _intRating = 1;
        private int _cachedLearnedRating;

        #region Constructor, Create, Save, Load, and Print Methods
        public Power(Character objCharacter)
        {
            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
            CharacterObject.PropertyChanged += OnCharacterChanged;
            if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
            {
                MAGAttributeObject = CharacterObject.MAGAdept;
            }
            else
            {
                MAGAttributeObject = CharacterObject.MAG;
            }
        }

        public void UnbindPower()
        {
            CharacterObject.PropertyChanged -= OnCharacterChanged;
            MAGAttributeObject = null;
            BoostedSkill = null;
        }

        public void DeletePower()
        {
            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId);
            CharacterObject.Powers.Remove(this);
            UnbindPower();
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
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", Extra);
            objWriter.WriteElementString("pointsperlevel", _strPointsPerLevel);
            objWriter.WriteElementString("adeptway", _strAdeptWayDiscount);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("extrapointcost", _decExtraPointCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
            objWriter.WriteElementString("maxlevel", _intMaxLevel.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
            objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
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
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetField("id", Guid.TryParse, out _sourceID);
            _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("points", ref _strPointsPerLevel);
            objNode.TryGetStringFieldQuickly("adeptway", ref _strAdeptWayDiscount);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            _intRating = intRating;
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
            XmlNode nodEnhancements = objNode["enhancements"];
            if (nodEnhancements != null)
            {
                using (XmlNodeList xmlEnhancementList = nodEnhancements.SelectNodes("enhancement"))
                    if (xmlEnhancementList != null)
                        foreach (XmlNode nodEnhancement in xmlEnhancementList)
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
                    DeletePower();
                    return false;
                }
                Extra = ImprovementManager.SelectedValue;
                ImprovementManager.SelectedValue = strOldSelected;
                ImprovementManager.ForcedValue = strOldForce;
            }
            if (TotalMaximumLevels < Rating)
            {
                Rating = TotalMaximumLevels;
            }
            return true;
        }

        /// <summary>
        /// Load the Power from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (objNode.TryGetField("id", Guid.TryParse, out _sourceID))
            {
                _objCachedMyXmlNode = null;
            }
            else
            {
                string strPowerName = Name;
                int intPos = strPowerName.IndexOf('(');
                if (intPos != -1)
                    strPowerName = strPowerName.Substring(0, intPos - 1);
                XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                XmlNode xmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                if (xmlPower.TryGetField("id", Guid.TryParse, out _sourceID))
                {
                    _objCachedMyXmlNode = null;
                }
            }
            Extra = objNode["extra"]?.InnerText ?? string.Empty;
            _strPointsPerLevel = objNode["pointsperlevel"]?.InnerText;
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            _strAdeptWayDiscount = objNode["adeptway"]?.InnerText;
            if (string.IsNullOrEmpty(_strAdeptWayDiscount))
            {
                string strPowerName = Name;
                int intPos = strPowerName.IndexOf('(');
                if (intPos != -1)
                    strPowerName = strPowerName.Substring(0, intPos - 1);
                _strAdeptWayDiscount = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]/adeptway")?.InnerText ?? string.Empty;
            }
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevel);
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
                XmlNode objXmlPower = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[starts-with(./name,\"Improved Reflexes\")]");
                if (objXmlPower != null)
                {
                    if (int.TryParse(Name.TrimStartOnce("Improved Reflexes", true).Trim(), out int intTemp))
                    {
                        Create(objXmlPower, intTemp, null, false);
                        objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
                    }
                }
            }
            else
            {
                XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
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
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
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

        private CharacterAttrib _objMAGAttribute;
        /// <summary>
        /// MAG Attribute this skill primarily depends on
        /// </summary>
        public CharacterAttrib MAGAttributeObject
        {
            get => _objMAGAttribute;
            protected set
            {
                if (_objMAGAttribute != value)
                {
                    if (_objMAGAttribute != null)
                        _objMAGAttribute.PropertyChanged -= OnLinkedAttributeChanged;
                    if (value != null)
                        value.PropertyChanged += OnLinkedAttributeChanged;
                    _objMAGAttribute = value;
                }
            }
        }

        private Skill _objBoostedSkill;

        public Skill BoostedSkill
        {
            get => _objBoostedSkill;
            protected set
            {
                if (_objBoostedSkill != value)
                {
                    if (_objBoostedSkill != null)
                        _objBoostedSkill.PropertyChanged -= OnBoostedSkillChanged;
                    if (value != null)
                        value.PropertyChanged += OnBoostedSkillChanged;
                    _objBoostedSkill = value;
                }
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this Power in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Power's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    if (Name == "Improved Ability (skill)")
                    {
                        BoostedSkill = null;
                    }
                    else if (value == "Improved Ability (skill)")
                    {
                        BoostedSkill = CharacterObject.SkillsSection.GetActiveSkill(Extra);
                    }
                    _strName = value;
                }
            }
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set
            {
                if (_strExtra != value)
                {
                    _strExtra = value;
                    if (Name == "Improved Ability (skill)")
                    {
                        BoostedSkill = CharacterObject.SkillsSection.GetActiveSkill(value);
                    }
                }
            }
        }

        /// <summary>
        /// The Enhancements currently applied to the Power.
        /// </summary>
        public TaggedObservableCollection<Enhancement> Enhancements { get; } = new TaggedObservableCollection<Enhancement>();

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
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
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
            set
            {
                string strNewValue = value.ToString(GlobalOptions.InvariantCultureInfo);
                if (_strPointsPerLevel != strNewValue)
                {
                    _strPointsPerLevel = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// An additional cost on top of the power's PointsPerLevel.
        /// Example: Improved Reflexes is properly speaking Rating + 0.5, but the math for that gets weird.
        /// </summary>
        public decimal ExtraPointCost
        {
            get => _decExtraPointCost;
            set
            {
                if (_decExtraPointCost != value)
                {
                    _decExtraPointCost = value;
                    OnPropertyChanged();
                }
            }
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
                string strNewValue = value.ToString(GlobalOptions.InvariantCultureInfo);
                if (_strAdeptWayDiscount != strNewValue)
                {
                    _strAdeptWayDiscount = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Calculated Power Point cost per level of the Power (including discounts).
        /// </summary>
        public decimal CalculatedPointsPerLevel => PointsPerLevel;

        /// <summary>
        /// Calculate the discount that is applied to the Power.
        /// </summary>
        private decimal Discount => DiscountedAdeptWay ? AdeptWayDiscount : 0;

        /// <summary>
        /// The current Rating of the Power.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                if (_intRating != value)
                {
                    _intRating = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The current Rating of the Power, including any Free Levels.
        /// </summary>
        public int TotalRating
        {
            get => Math.Min(Rating + FreeLevels, TotalMaximumLevels);
            set => Rating = Math.Max(value - FreeLevels, 0);
        }

        public bool DoesNotHaveFreeLevels => FreeLevels == 0;

        private int _intCachedFreeLevels = int.MinValue;

        /// <summary>
        /// Free levels of the power.
        /// </summary>
        public int FreeLevels
        {
            get
            {
                if (_intCachedFreeLevels != int.MinValue)
                    return _intCachedFreeLevels;

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
                return _intCachedFreeLevels = Math.Min(intReturn, MAGAttributeObject.TotalValue);
            }
        }

        private decimal _decCachedPowerPoints = decimal.MinValue;
        /// <summary>
        /// Total number of Power Points the Power costs.
        /// </summary>
        public decimal PowerPoints
        {
            get
            {
                if (_decCachedPowerPoints != decimal.MinValue)
                    return _decCachedPowerPoints;

                if (Rating == 0 || !LevelsEnabled && FreeLevels > 0)
                {
                    return _decCachedPowerPoints = 0;
                }

                decimal decReturn;
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
                return _decCachedPowerPoints = Math.Max(decReturn, 0.0m);
            }
        }

        public string DisplayPoints
        {
            get
            {
                if (string.IsNullOrEmpty(_strCachedPowerPoints))
                    _strCachedPowerPoints = PowerPoints.ToString(GlobalOptions.CultureInfo);
                return _strCachedPowerPoints;
            }
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
        public bool LevelsEnabled
        {
            get => _blnLevelsEnabled;
            set
            {
                if (_blnLevelsEnabled != value)
                {
                    _blnLevelsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum Level for the Power.
        /// </summary>
        public int MaxLevels
        {
            get => _intMaxLevel;
            set
            {
                if (_intMaxLevel != value)
                {
                    _intMaxLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 50% from Adept Way.
        /// </summary>
        public bool DiscountedAdeptWay
        {
            get => _blnDiscountedAdeptWay;
            set
            {
                if (value != _blnDiscountedAdeptWay)
                {
                    _blnDiscountedAdeptWay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 25% from Geas.
        /// </summary>
        public bool DiscountedGeas
        {
            get => _blnDiscountedGeas;
            set
            {
                if (value != _blnDiscountedGeas)
                {
                    _blnDiscountedGeas = value;
                    OnPropertyChanged();
                }
            }
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

            switch (Action)
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
                    // if unspecified, max rating = MAG
                    intReturn = MAGAttributeObject.TotalValue;
                }
                if (BoostedSkill != null)
                {
                    // +1 at the end so that division of 2 always rounds up, and integer division by 2 is significantly less expensive than decimal/double division
                    intReturn = Math.Min(intReturn, (BoostedSkill.LearnedRating + 1) / 2);
                }
                if (!CharacterObject.IgnoreRules)
                {
                    intReturn = Math.Min(intReturn, MAGAttributeObject.TotalValue);
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
                    blnReturn = _nodAdeptWayRequirements.RequirementsMet(CharacterObject);
                }

                return blnReturn;
            }
        }

        public void RefreshDiscountedAdeptWay(bool blnAdeptWayDiscountEnabled)
        {
            if (DiscountedAdeptWay && !blnAdeptWayDiscountEnabled)
                DiscountedAdeptWay = false;
        }

        private static readonly DependancyGraph<string> PowerDependencyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(DisplayPoints),
                    new DependancyGraphNode<string>(nameof(PowerPoints),
                        new DependancyGraphNode<string>(nameof(TotalRating),
                            new DependancyGraphNode<string>(nameof(Rating)),
                            new DependancyGraphNode<string>(nameof(FreeLevels),
                                new DependancyGraphNode<string>(nameof(FreePoints)),
                                new DependancyGraphNode<string>(nameof(ExtraPointCost)),
                                new DependancyGraphNode<string>(nameof(PointsPerLevel))
                            ),
                            new DependancyGraphNode<string>(nameof(TotalMaximumLevels),
                                new DependancyGraphNode<string>(nameof(LevelsEnabled)),
                                new DependancyGraphNode<string>(nameof(MaxLevels))
                            )
                        ),
                        new DependancyGraphNode<string>(nameof(Rating)),
                        new DependancyGraphNode<string>(nameof(LevelsEnabled)),
                        new DependancyGraphNode<string>(nameof(FreeLevels)),
                        new DependancyGraphNode<string>(nameof(PointsPerLevel)),
                        new DependancyGraphNode<string>(nameof(FreePoints)),
                        new DependancyGraphNode<string>(nameof(ExtraPointCost)),
                        new DependancyGraphNode<string>(nameof(Discount),
                            new DependancyGraphNode<string>(nameof(DiscountedAdeptWay)),
                            new DependancyGraphNode<string>(nameof(AdeptWayDiscount))
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(ToolTip),
                    new DependancyGraphNode<string>(nameof(Rating)),
                    new DependancyGraphNode<string>(nameof(PointsPerLevel))
                ),
                new DependancyGraphNode<string>(nameof(DoesNotHaveFreeLevels),
                    new DependancyGraphNode<string>(nameof(FreeLevels))
                )
            );

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
                    lstNamesOfChangedProperties = PowerDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in PowerDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(DisplayPoints)))
                _strCachedPowerPoints = string.Empty;
            if (lstNamesOfChangedProperties.Contains(nameof(FreeLevels)))
                _intCachedFreeLevels = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(PowerPoints)))
                _decCachedPowerPoints = decimal.MinValue;

            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
            if (lstNamesOfChangedProperties.Contains(nameof(TotalRating)))
            {
                if (Bonus?.InnerXml.Contains("Rating") == true)
                {
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId);
                    int intTotalRating = TotalRating;
                    if (intTotalRating > 0)
                    {
                        ImprovementManager.ForcedValue = Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, InternalId, Bonus, false, intTotalRating, DisplayNameShort(GlobalOptions.Language));
                    }
                }
            }

            if (PropertyChanged != null)
            {
                foreach (string strPropertyToChange in lstNamesOfChangedProperties)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(TotalMaximumLevels));
        }

        protected void OnBoostedSkillChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Skill.LearnedRating))
            {
                if (BoostedSkill.LearnedRating != _cachedLearnedRating && _cachedLearnedRating != TotalMaximumLevels)
                {
                    _cachedLearnedRating = ((Skill)sender).LearnedRating;
                    OnPropertyChanged(nameof(TotalMaximumLevels));
                }
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Character.IsMysticAdept))
            {
                if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                {
                    MAGAttributeObject = CharacterObject.MAGAdept;
                }
                else
                {
                    MAGAttributeObject = CharacterObject.MAG;
                }
            }
        }
        
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("powers.xml", strLanguage).SelectSingleNode("/chummer/powers/power[id = \"" + _sourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// ToolTip that shows how the Power is calculating its Modified Rating.
        /// </summary>
        public string ToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder strbldModifier = new StringBuilder("Rating" + strSpaceCharacter + '(' + Rating.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '×' + strSpaceCharacter + PointsPerLevel.ToString(GlobalOptions.CultureInfo) + ')');
                foreach (Improvement objImprovement in CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower && objImprovement.ImprovedName == Name && objImprovement.UniqueName == Extra && objImprovement.Enabled))
                {
                    strbldModifier.Append(strSpaceCharacter + '+' + strSpaceCharacter + CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language) + strSpaceCharacter + '(' + objImprovement.Rating.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return strbldModifier.ToString();
            }
        }
        #endregion
    }
}
