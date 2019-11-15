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
using System.Windows.Forms;
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
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameMethod(GlobalOptions.DefaultLanguage)}")]
    public class Power : INotifyMultiplePropertyChanged, IHasInternalId, IHasName, IHasXmlNode, IHasNotes, IHasSource
    {
        internal Guid _guiID;
        internal Guid _guiSourceID = Guid.Empty;
        internal string _strName = string.Empty;
        internal string _strExtra = string.Empty;
        internal string _strSource = string.Empty;
        internal string _strPage = string.Empty;
        internal string _strPointsPerLevel = "0";
        internal string _strAction = string.Empty;
        internal decimal _decExtraPointCost;
        internal int _intMaxLevels;
        internal bool _blnDiscountedAdeptWay;
        internal bool _blnDiscountedGeas;
        internal XmlNode _nodAdeptWayRequirements;
        internal string _strNotes = string.Empty;
        internal string _strAdeptWayDiscount = "0";
        internal string _strBonusSource = string.Empty;
        internal decimal _decFreePoints;
        internal string _strCachedPowerPoints = string.Empty;
        internal bool _blnLevelsEnabled;
        internal int _intRating = 1;
        internal int _cachedLearnedRating;
        internal XmlNode _objCachedMyXmlNode;
        internal string _strCachedXmlNodeLanguage = string.Empty;
        internal string _strRange;
        internal string _strDuration;
        internal Improvement.ImprovementSource _improvementSource = Improvement.ImprovementSource.Power;
        internal Improvement.ImprovementType _freeLevelImprovementType;
        internal Improvement.ImprovementType _freePointImprovementType;
        internal Improvement.ImprovementType _improvementType;

        #region Constructor, Create, Save, Load, and Print Methods
        public Power(Character objCharacter)
        {
            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
            CharacterObject.PropertyChanged += OnCharacterChanged;
        }

        public void UnbindPower()
        {
            CharacterObject.PropertyChanged -= OnCharacterChanged;
            MAGAttributeObject = null;
            BoostedSkill = null;
        }

        protected void OnCharacterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.IsMysticAdept))
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

        public void DeletePower()
        {
            ImprovementManager.RemoveImprovements(CharacterObject, _improvementSource, InternalId);
            switch (_improvementSource)
            {
                case Improvement.ImprovementSource.Power:
                    CharacterObject.Powers.Remove(this as AdeptPower);
                    break;
                case Improvement.ImprovementSource.CritterPower:
                    CharacterObject.CritterPowers.Remove(this as CritterPower);
                    break;
            }

            UnbindPower();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            switch (this)
            {
                case AdeptPower objPower:
                    objPower.OnPropertyChanged(strPropertyName);
                    break;
                case CritterPower objPower:
                    objPower.OnPropertyChanged(strPropertyName);
                    break;
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

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            DependancyGraph<string> objDependencyGraph = null;
            switch (this)
            {
                case CritterPower _:
                    objDependencyGraph = CritterPower.PowerDependencyGraph;
                    break;
                case AdeptPower _:
                default:
                    objDependencyGraph = AdeptPower.PowerDependencyGraph;
                    break;
            }
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = objDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in objDependencyGraph.GetWithAllDependants(strPropertyName))
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
                    ImprovementManager.RemoveImprovements(CharacterObject, _improvementSource, InternalId);
                    int intTotalRating = TotalRating;
                    if (intTotalRating > 0)
                    {
                        ImprovementManager.ForcedValue = Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, _improvementSource, InternalId, Bonus, false, intTotalRating, DisplayNameShort(GlobalOptions.Language));
                    }
                }
            }

            if (this is AdeptPower objPower && lstNamesOfChangedProperties.Contains(nameof(AdeptPower.AdeptWayDiscountEnabled)))
            {
                objPower.RefreshDiscountedAdeptWay(objPower.AdeptWayDiscountEnabled);
            }

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language));
        #endregion

        #region Properties
        /// <summary>
        /// The Character object being used by the Power.
        /// </summary>
        public Character CharacterObject { get; set; }

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
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D");

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
        /// The current 'paid' Rating of the Power.
        /// </summary>
        public int Rating
        {
            get
            {
                //TODO: This isn't super safe, but it's more reliable than checking it at load as improvement effects like Essence Loss take effect after powers are loaded. Might need another solution.
                if (_intRating <= TotalMaximumLevels) return _intRating;
                _intRating = TotalMaximumLevels;
                return _intRating;
            }
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

        internal int _intCachedFreeLevels = int.MinValue;

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
                int intReturn = CharacterObject.Improvements.Where(objImprovement =>
                        objImprovement.ImproveType == _freeLevelImprovementType &&
                        objImprovement.ImprovedName == Name &&
                        objImprovement.UniqueName == Extra && objImprovement.Enabled)
                    .Sum(objImprovement => objImprovement.Rating);
                if (PointsPerLevel <= 0)
                    return _intCachedFreeLevels = Math.Min(intReturn, MAGAttributeObject?.TotalValue ?? 0);
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

                return _intCachedFreeLevels = Math.Min(intReturn, MAGAttributeObject?.TotalValue ?? 0);
            }
        }

        internal decimal _decCachedPowerPoints = decimal.MinValue;
        /// <summary>
        /// Total number of Power Points the Power costs.
        /// </summary>
        public virtual decimal PowerPoints
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

                if (this is AdeptPower objPower)
                {
                    decReturn -= objPower.Discount;
                }

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
                int intRating = CharacterObject.Improvements.Where(objImprovement =>
                        objImprovement.ImproveType == _freePointImprovementType &&
                        objImprovement.ImprovedName == Name &&
                        objImprovement.UniqueName == Extra && objImprovement.Enabled)
                    .Sum(objImprovement => objImprovement.Rating);
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
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
            get => _intMaxLevels;
            set
            {
                if (_intMaxLevels == value) return;
                _intMaxLevels = value;
                OnPropertyChanged();
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

        /// <summary>
        /// Range.
        /// </summary>
        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strReturn = Range;

            strReturn = strReturn.CheapReplace("Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage))
                .CheapReplace("Special", () => LanguageManager.GetString("String_SpellDurationSpecial", strLanguage))
                .CheapReplace("LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage))
                .CheapReplace("LOI", () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage))
                .CheapReplace("T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage))
                .CheapReplace("(A)", () => '(' + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')')
                .CheapReplace("MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage));

            return strReturn;
        }

        /// <summary>
        /// Duration.
        /// </summary>
        public string Duration
        {
            get => _strDuration;
            set => _strDuration = value;
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayDuration(string strLanguage)
        {
            string strReturn = Duration;

            switch (strReturn)
            {
                case "Instant":
                    strReturn = LanguageManager.GetString("String_SpellDurationInstantLong", strLanguage);
                    break;
                case "Sustained":
                    strReturn = LanguageManager.GetString("String_SpellDurationSustained", strLanguage);
                    break;
                case "Always":
                    strReturn = LanguageManager.GetString("String_SpellDurationAlways", strLanguage);
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
                    // if unspecified, max rating = MAG
                    intReturn = MAGAttributeObject?.TotalValue ?? 0;
                }
                if (BoostedSkill != null)
                {
                    // +1 at the end so that division of 2 always rounds up, and integer division by 2 is significantly less expensive than decimal/double division
                    intReturn = Math.Min(intReturn, ( + (BoostedSkill.LearnedRating + 1)) / 2);
                    if (CharacterObject.Options.IncreasedImprovedAbilityMultiplier)
                    {
                        intReturn += BoostedSkill.LearnedRating;
                    }
                }
                if (!CharacterObject.IgnoreRules)
                {
                    intReturn = Math.Min(intReturn, MAGAttributeObject?.TotalValue ?? 0);
                }
                return intReturn;
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
                XmlDocument xmlDoc = null;

                if (this is AdeptPower)
                {
                    xmlDoc = XmlManager.Load("powers.xml", strLanguage);
                }
                else if (this is CritterPower)
                {
                    XmlManager.Load("critterpowers.xml", strLanguage);
                }
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? xmlDoc.SelectSingleNode($"/chummer/powers/power[name = \"{Name}\"]")
                    : xmlDoc.SelectSingleNode($"/chummer/powers/power[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
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
                StringBuilder strbldModifier =
                    new StringBuilder("Rating" + strSpaceCharacter + '(' + Rating.ToString(GlobalOptions.CultureInfo) +
                                      strSpaceCharacter + '×' + strSpaceCharacter +
                                      PointsPerLevel.ToString(GlobalOptions.CultureInfo) + ')');
                foreach (Improvement objImprovement in CharacterObject.Improvements.Where(objImprovement =>
                    objImprovement.ImproveType == _improvementType && objImprovement.ImprovedName == Name &&
                    objImprovement.UniqueName == Extra && objImprovement.Enabled))
                {
                    strbldModifier.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objImprovement.Rating.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return strbldModifier.ToString();
            }
        }
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
        public TreeNode CreateTreeNode(ContextMenuStrip cmsQuality)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayNameMethod(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsQuality,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            return objNode;
        }
    }
}
