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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strGroupName} {_intSkillFromSp} {_intSkillFromKarma}")]
    public sealed class SkillGroup : INotifyMultiplePropertyChanged, IHasInternalId, IHasName, IEquatable<SkillGroup>
    {
        #region Core calculations

        private int _intSkillFromSp;
        private int _intSkillFromKarma;
        private bool _blnIsBroken;

        public int Base
        {
            get => IsDisabled ? 0 : Math.Min(BasePoints + FreeBase, RatingMaximum);
            set
            {
                if (!BaseUnbroken)
                    return;
                //Calculate how far above maximum we are.
                int intOverMax = (value + KarmaPoints + FreeLevels) - RatingMaximum;

                //reduce value by max or 0
                value -= Math.Max(0, intOverMax);

                //and save back, cannot go under 0
                BasePoints = Math.Max(0, value - FreeBase);
                foreach (Skill skill in SkillList)
                {
                    //To trigger new calculation of skill.KarmaPoints
                    skill.OnPropertyChanged(nameof(Skill.Base));
                }
            }
        }

        public int Karma
        {
            get
            {
                if (!KarmaUnbroken && KarmaPoints > 0)
                {
                    KarmaPoints = 0;
                }
                return IsDisabled ? 0 : Math.Min(KarmaPoints + FreeLevels, RatingMaximum);
            }
            set
            {
                if (!KarmaUnbroken)
                    return;
                //Calculate how far above maximum we are.
                int intOverMax = value + BasePoints + FreeBase - RatingMaximum;

                //reduce value by max or 0
                value -= Math.Max(0, intOverMax);

                //and save back, cannot go under 0
                KarmaPoints = Math.Max(0, value - FreeLevels);
                foreach (Skill skill in SkillList)
                {
                    //To trigger new calculation of skill.KarmaPoints
                    skill.OnPropertyChanged(nameof(Skill.Karma));
                }
            }
        }

        /// <summary>
        /// Amount of Base that has been provided by non-Improvement sources.
        /// </summary>
        public int BasePoints
        {
            get => _intSkillFromSp;
            set
            {
                if (_intSkillFromSp == value)
                    return;
                _intSkillFromSp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Amount of Karma Levels that has been provided by non-Improvement sources.
        /// </summary>
        public int KarmaPoints
        {
            get => _intSkillFromKarma;
            set
            {
                if (_intSkillFromKarma == value)
                    return;
                _intSkillFromKarma = value;
                OnPropertyChanged();
            }
        }

        private int _intCachedBaseUnbroken = int.MinValue;

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplify databinding
        /// </summary>
        public bool BaseUnbroken
        {
            get
            {
                if (_intCachedBaseUnbroken < 0)
                {
                    if (IsDisabled || SkillList.Count == 0 || !_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                        _intCachedBaseUnbroken = 0;
                    else if (_objCharacter.Settings.StrictSkillGroupsInCreateMode && !_objCharacter.Created)
                        _intCachedBaseUnbroken =
                            SkillList.All(x => x.BasePoints + x.FreeBase <= 0)
                            && SkillList.All(x => x.KarmaPoints + x.FreeKarma <= 0)
                                ? 1 : 0;
                    else if (_objCharacter.Settings.UsePointsOnBrokenGroups)
                        _intCachedBaseUnbroken = KarmaUnbroken ? 1 : 0;
                    else
                        _intCachedBaseUnbroken = SkillList.All(x => x.BasePoints + x.FreeBase <= 0) ? 1 : 0;
                }
                return _intCachedBaseUnbroken > 0;
            }
        }

        private int _intCachedKarmaUnbroken = int.MinValue;

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplify databinding
        /// </summary>
        public bool KarmaUnbroken
        {
            get
            {
                if (_intCachedKarmaUnbroken < 0)
                {
                    if (IsDisabled || SkillList.Count == 0)
                        _intCachedKarmaUnbroken = 0;
                    else if (_objCharacter.Settings.StrictSkillGroupsInCreateMode && !_objCharacter.Created)
                        _intCachedKarmaUnbroken = SkillList.All(x => x.BasePoints + x.FreeBase <= 0)
                                                  && SkillList.All(x => x.KarmaPoints + x.FreeKarma <= 0)
                            ? 1
                            : 0;
                    else
                    {
                        int intHigh = SkillList.Max(x => x.BasePoints + x.FreeBase);

                        _intCachedKarmaUnbroken = SkillList.All(x => x.BasePoints + x.FreeBase + x.KarmaPoints + x.FreeKarma >= intHigh)
                            ? 1
                            : 0;
                    }
                }
                return _intCachedKarmaUnbroken > 0;
            }
        }

        private int _intCachedIsDisabled = int.MinValue;

        public bool IsDisabled
        {
            get
            {
                if (_intCachedIsDisabled < 0)
                {
                    _intCachedIsDisabled = ImprovementManager
                                           .GetCachedImprovementListForValueOf(
                                               _objCharacter, Improvement.ImprovementType.SkillGroupDisable, Name)
                                           .Count > 0
                                           || ImprovementManager
                                              .GetCachedImprovementListForValueOf(
                                                  _objCharacter, Improvement.ImprovementType.SkillGroupCategoryDisable)
                                              .Any(
                                                  x => GetRelevantSkillCategories.Contains(x.ImprovedName))
                        ? 1
                        : 0;
                }
                return _intCachedIsDisabled > 0;
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        public bool IsBroken
        {
            get => _blnIsBroken;
            private set
            {
                if (_blnIsBroken == value)
                    return;
                _blnIsBroken = value;
                OnPropertyChanged();
            }
        }

        private void UpdateIsBroken()
        {
            if (!_objCharacter.Created)
                return;
            if (!_objCharacter.Settings.AllowSkillRegrouping && IsBroken)
                return;
            IsBroken = HasAnyBreakingSkills;
        }

        private int _intCachedHasAnyBreakingSkills = int.MinValue;

        public bool HasAnyBreakingSkills
        {
            get
            {
                if (_intCachedHasAnyBreakingSkills < 0)
                {
                    if (SkillList.Count <= 1)
                        _intCachedHasAnyBreakingSkills = 0;
                    else
                    {
                        Skill objFirstEnabledSkill = SkillList.Find(x => x.Enabled);
                        if (objFirstEnabledSkill == null || SkillList.All(x => x == objFirstEnabledSkill || !x.Enabled))
                            _intCachedHasAnyBreakingSkills = 0;
                        else if (_objCharacter.Settings.SpecializationsBreakSkillGroups && SkillList.Any(x => x.Specializations.Count != 0
                                     && x.Enabled))
                        {
                            _intCachedHasAnyBreakingSkills = 1;
                        }
                        else
                        {
                            int intFirstSkillTotalBaseRating = objFirstEnabledSkill.TotalBaseRating;
                            _intCachedHasAnyBreakingSkills = SkillList.Any(x => x != objFirstEnabledSkill
                                                                               && x.TotalBaseRating
                                                                               != intFirstSkillTotalBaseRating
                                                                               && x.Enabled)
                                ? 1
                                : 0;
                        }
                    }
                }
                return _intCachedHasAnyBreakingSkills > 0;
            }
        }

        public bool CareerCanIncrease => UpgradeKarmaCost <= CharacterObject.Karma && !IsDisabled && !IsBroken;

        public int Rating => Karma + Base;

        public int FreeBase =>
            !string.IsNullOrEmpty(Name)
                ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupBase, false, Name).StandardRound()
                : 0;

        public int FreeLevels =>
            !string.IsNullOrEmpty(Name)
                ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupLevel, false, Name).StandardRound()
                : 0;

        public int RatingMaximum => _objCharacter.Created || _objCharacter.IgnoreRules
            ? _objCharacter.Settings.MaxSkillRating
            : _objCharacter.Settings.MaxSkillRatingCreate;

        public void Upgrade()
        {
            if (CharacterObject.Created)
            {
                if (IsBroken)
                    return;

                int intPrice = UpgradeKarmaCost;

                //If data file contains {4} this crashes but...
                string strUpgradetext =
                    string.Format(GlobalSettings.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                        LanguageManager.GetString("String_ExpenseSkillGroup"), CurrentDisplayName,
                        Rating, Rating + 1, LanguageManager.GetString("String_Space"));

                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(intPrice * -1, strUpgradetext, ExpenseType.Karma, DateTime.Now);
                objExpense.Undo = new ExpenseUndo().CreateKarma(Rating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Name);

                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                CharacterObject.Karma -= intPrice;
            }

            ++Karma;
        }

        #endregion Core calculations

        #region All the other stuff that is required

        public static SkillGroup Get(Skill objSkill)
        {
            if (objSkill == null)
                return null;
            if (objSkill.SkillGroupObject != null)
                return objSkill.SkillGroupObject;

            if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                return null;
            
            foreach (SkillGroup objSkillGroup in objSkill.CharacterObject.SkillsSection.SkillGroups)
            {
                if (objSkillGroup.Name == objSkill.SkillGroup)
                {
                    if (!objSkillGroup.SkillList.Contains(objSkill))
                        objSkillGroup.Add(objSkill);
                    return objSkillGroup;
                }
            }

            SkillGroup objNewGroup = new SkillGroup(objSkill.CharacterObject, objSkill.SkillGroup);
            objNewGroup.Add(objSkill);
            objSkill.CharacterObject.SkillsSection.SkillGroups.AddWithSort(objNewGroup, SkillsSection.CompareSkillGroups,
                (objExistingSkillGroup, objNewSkillGroup) =>
                {
                    foreach (Skill x in objExistingSkillGroup.SkillList.Where(x => !objExistingSkillGroup.SkillList.Contains(x)))
                        objExistingSkillGroup.Add(x);
                    objNewSkillGroup.UnbindSkillGroup();
                });

            return objNewGroup;
        }

        public void Add(Skill skill)
        {
            _lstAffectedSkills.Add(skill);
            skill.PropertyChanged += SkillOnPropertyChanged;
            OnPropertyChanged(nameof(SkillList));
        }

        internal void WriteTo(XmlWriter writer)
        {
            if (writer == null)
                return;
            writer.WriteStartElement("group");

            writer.WriteElementString("karma", _intSkillFromKarma.ToString(GlobalSettings.InvariantCultureInfo));
            writer.WriteElementString("base", _intSkillFromSp.ToString(GlobalSettings.InvariantCultureInfo));
            writer.WriteElementString("isbroken", _blnIsBroken.ToString(GlobalSettings.InvariantCultureInfo));
            writer.WriteElementString("id", _guidId.ToString("D", GlobalSettings.InvariantCultureInfo));
            writer.WriteElementString("name", _strGroupName);

            writer.WriteEndElement();
        }

        internal void Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("skillgroup");

            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratingmax", RatingMaximum.ToString(objCulture));
            objWriter.WriteElementString("base", Base.ToString(objCulture));
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));
            objWriter.WriteElementString("isbroken", IsBroken.ToString(GlobalSettings.InvariantCultureInfo));

            objWriter.WriteEndElement();
        }

        public void Load(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid g) && g != Guid.Empty)
                _guidId = g;
            xmlNode.TryGetStringFieldQuickly("name", ref _strGroupName);
            xmlNode.TryGetInt32FieldQuickly("karma", ref _intSkillFromKarma);
            xmlNode.TryGetInt32FieldQuickly("base", ref _intSkillFromSp);
            xmlNode.TryGetBoolFieldQuickly("isbroken", ref _blnIsBroken);
        }

        public void LoadFromHeroLab(XPathNavigator xmlNode)
        {
            if (xmlNode == null)
                return;
            string strTemp = xmlNode.SelectSingleNode("@name")?.Value;
            if (!string.IsNullOrEmpty(strTemp))
                _strGroupName = strTemp.TrimEndOnce("Group").Trim();
            strTemp = xmlNode.SelectSingleNode("@base")?.Value;
            if (!string.IsNullOrEmpty(strTemp) && int.TryParse(strTemp, out int intTemp))
                _intSkillFromKarma = intTemp;
        }

        private static readonly PropertyDependencyGraph<SkillGroup> s_SkillGroupDependencyGraph =
            new PropertyDependencyGraph<SkillGroup>(
                new DependencyGraphNode<string, SkillGroup>(nameof(DisplayRating),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    ),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsBroken),
                        new DependencyGraphNode<string, SkillGroup>(nameof(HasAnyBreakingSkills), x => !x.IsBroken || x.CharacterObject.Settings.AllowSkillRegrouping,
                            new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                        )
                    ),
                    new DependencyGraphNode<string, SkillGroup>(nameof(Rating),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Karma),
                            new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(RatingMaximum)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(FreeLevels),
                                new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                            ),
                            new DependencyGraphNode<string, SkillGroup>(nameof(KarmaPoints)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(KarmaUnbroken),
                                new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                                new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                            )
                        ),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Base),
                            new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(RatingMaximum)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(FreeBase),
                                new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                            ),
                            new DependencyGraphNode<string, SkillGroup>(nameof(BasePoints))
                        )
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeToolTip),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Rating)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CareerCanIncrease),
                    new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeKarmaCost)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsBroken))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(BaseUnbroken),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(KarmaUnbroken), x => x._objCharacter.Settings.UsePointsOnBrokenGroups)
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(ToolTip),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, SkillGroup>(nameof(DisplayName),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentSpCost),
                    new DependencyGraphNode<string, SkillGroup>(nameof(BasePoints)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentKarmaCost),
                    new DependencyGraphNode<string, SkillGroup>(nameof(KarmaPoints)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                )
            );

        private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Skill.BasePoints):
                case nameof(Skill.FreeBase):
                    this.OnMultiplePropertyChanged(nameof(BaseUnbroken), nameof(KarmaUnbroken));
                    break;

                case nameof(Skill.KarmaPoints):
                case nameof(Skill.FreeKarma):
                    OnPropertyChanged(nameof(KarmaUnbroken));
                    break;

                case nameof(Skill.Specializations):
                    if (CharacterObject.Settings.SpecializationsBreakSkillGroups)
                        OnPropertyChanged(nameof(HasAnyBreakingSkills));
                    break;

                case nameof(Skill.TotalBaseRating):
                case nameof(Skill.Enabled):
                    this.OnMultiplePropertyChanged(nameof(HasAnyBreakingSkills),
                                                   nameof(DisplayRating),
                                                   nameof(UpgradeToolTip),
                                                   nameof(CurrentKarmaCost),
                                                   nameof(UpgradeKarmaCost));
                    break;
            }
        }

        private readonly List<Skill> _lstAffectedSkills = new List<Skill>(4);
        private string _strGroupName;
        private readonly Character _objCharacter;

        public SkillGroup(Character objCharacter, string strGroupName = "")
        {
            _objCharacter = objCharacter;
            _strGroupName = strGroupName;
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged += OnCharacterPropertyChanged;
                _objCharacter.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            }
        }

        public void UnbindSkillGroup()
        {
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
                _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
            }
            foreach (Skill objSkill in _lstAffectedSkills)
                objSkill.PropertyChanged -= SkillOnPropertyChanged;
        }

        public Character CharacterObject => _objCharacter;

        public string Name
        {
            get => _strGroupName;
            set
            {
                if (value == _strGroupName)
                    return;
                _strGroupName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string DisplayName(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;
            return _objCharacter.LoadDataXPath("skills.xml", strLanguage).SelectSingleNode("/chummer/skillgroups/name[. = " + Name.CleanXPath() + "]/@translate")?.Value ?? Name;
        }

        public string DisplayRating
        {
            get
            {
                if (IsDisabled)
                    return LanguageManager.GetString("Label_SkillGroup_Disabled");
                if (IsBroken)
                    return LanguageManager.GetString("Label_SkillGroup_Broken");
                int intReturn = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (objSkill.Enabled)
                        intReturn = Math.Min(intReturn, objSkill.TotalBaseRating);
                }
                return intReturn == int.MaxValue
                    ? 0.ToString(GlobalSettings.CultureInfo)
                    : intReturn.ToString(GlobalSettings.CultureInfo);
            }
        }

        private string _strToolTip = string.Empty;

        public string ToolTip
        {
            get
            {
                if (!string.IsNullOrEmpty(_strToolTip))
                    return _strToolTip;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTooltip))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    sbdTooltip.Append(LanguageManager.GetString("Tip_SkillGroup_Skills")).Append(strSpace)
                              .AppendJoin(',' + strSpace, SkillList.Select(x => x.CurrentDisplayName)).AppendLine();

                    if (IsDisabled)
                    {
                        sbdTooltip.AppendLine(LanguageManager.GetString("Label_SkillGroup_DisabledBy"));
                        List<Improvement> lstImprovements
                            = ImprovementManager.GetCachedImprovementListForValueOf(
                                _objCharacter, Improvement.ImprovementType.SkillGroupDisable, Name);
                        lstImprovements.AddRange(ImprovementManager.GetCachedImprovementListForValueOf(
                                                                       _objCharacter,
                                                                       Improvement.ImprovementType
                                                                           .SkillGroupCategoryDisable)
                                                                   .Where(x => GetRelevantSkillCategories.Contains(
                                                                              x.ImprovedName)));
                        foreach (Improvement objImprovement in lstImprovements)
                        {
                            sbdTooltip.AppendLine(CharacterObject.GetObjectName(objImprovement));
                        }
                    }

                    return _strToolTip = sbdTooltip.ToString();
                }
            }
        }

        public string UpgradeToolTip
        {
            get
            {
                int intRating = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (objSkill.Enabled)
                        intRating = Math.Min(intRating, objSkill.TotalBaseRating);
                }

                if (intRating == int.MaxValue)
                    intRating = 0;
                return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveItem"),
                                     intRating + 1, UpgradeKarmaCost);
            }
        }

        private Guid _guidId = Guid.NewGuid();

        public Guid Id => _guidId;

        public string InternalId => Id.ToString("D", GlobalSettings.InvariantCultureInfo);

        #region HasWhateverSkills

        public bool HasCombatSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Combat Active"); }
        }

        public bool HasPhysicalSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Physical Active"); }
        }

        public bool HasSocialSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Social Active"); }
        }

        public bool HasTechnicalSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Technical Active"); }
        }

        public bool HasVehicleSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Vehicle Active"); }
        }

        public bool HasMagicalSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Magical Active"); }
        }

        public bool HasResonanceSkills
        {
            get { return SkillList.Any(x => x.SkillCategory == "Resonance Active"); }
        }

        public IEnumerable<string> GetRelevantSkillCategories
        {
            get { return SkillList.Select(x => x.SkillCategory).Distinct(); }
        }

        #endregion HasWhateverSkills

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
                            = s_SkillGroupDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                    else
                    {
                        foreach (string strLoopChangedProperty in s_SkillGroupDependencyGraph
                                     .GetWithAllDependentsEnumerable(this, strPropertyName))
                            setNamesOfChangedProperties.Add(strLoopChangedProperty);
                    }
                }

                if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                    return;
                
                if (setNamesOfChangedProperties.Contains(nameof(IsDisabled)))
                    _intCachedIsDisabled = int.MinValue;
                if (setNamesOfChangedProperties.Contains(nameof(KarmaUnbroken)))
                    _intCachedKarmaUnbroken = int.MinValue;
                if (setNamesOfChangedProperties.Contains(nameof(BaseUnbroken)))
                    _intCachedBaseUnbroken = int.MinValue;
                if (setNamesOfChangedProperties.Contains(nameof(ToolTip)))
                    _strToolTip = string.Empty;
                if (setNamesOfChangedProperties.Contains(nameof(HasAnyBreakingSkills)))
                {
                    _intCachedHasAnyBreakingSkills = int.MinValue;
                    UpdateIsBroken();
                }

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

        private void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Character.Karma):
                    OnPropertyChanged(nameof(CareerCanIncrease));
                    break;

                case nameof(Character.EffectiveBuildMethodUsesPriorityTables):
                    OnPropertyChanged(nameof(BaseUnbroken));
                    break;
            }
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.StrictSkillGroupsInCreateMode):
                case nameof(CharacterSettings.UsePointsOnBrokenGroups):
                    OnPropertyChanged(nameof(BaseUnbroken));
                    break;

                case nameof(CharacterSettings.KarmaNewSkillGroup):
                case nameof(CharacterSettings.KarmaImproveSkillGroup):
                    OnPropertyChanged(nameof(CurrentKarmaCost));
                    break;

                case nameof(CharacterSettings.AllowSkillRegrouping):
                    UpdateIsBroken();
                    break;

                case nameof(CharacterSettings.SpecializationsBreakSkillGroups):
                    OnPropertyChanged(nameof(HasAnyBreakingSkills));
                    break;

                case nameof(CharacterSettings.MaxSkillRating):
                    if (_objCharacter.Created || _objCharacter.IgnoreRules)
                        OnPropertyChanged(nameof(RatingMaximum));
                    break;

                case nameof(CharacterSettings.MaxSkillRatingCreate):
                    if (!_objCharacter.Created && !_objCharacter.IgnoreRules)
                        OnPropertyChanged(nameof(RatingMaximum));
                    break;
            }
        }

        public int CurrentSpCost
        {
            get
            {
                int intReturn = BasePoints;
                int intValue = intReturn;
                
                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                    {
                        if ((objLoopImprovement.Maximum == 0 || intValue <= objLoopImprovement.Maximum)
                            && objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupPointCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                        intValue,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillGroupPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryPointCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                        intValue,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                else
                    intReturn += decExtra.StandardRound();

                return Math.Max(intReturn, 0);
            }
        }

        public int CurrentKarmaCost
        {
            get
            {
                if (KarmaPoints == 0)
                    return 0;

                int intUpper = SkillList.Min(x => x.TotalBaseRating);
                int intLower = intUpper - KarmaPoints;

                int intCost = intUpper * (intUpper + 1);
                intCost -= intLower * (intLower + 1);
                intCost /= 2; //We get square, need triangle

                if (intCost == 1)
                    intCost *= _objCharacter.Settings.KarmaNewSkillGroup;
                else
                    intCost *= _objCharacter.Settings.KarmaImproveSkillGroup;
                
                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                    {
                        if (objLoopImprovement.Minimum <= intLower &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition)
                             || (objLoopImprovement.Condition == "career") == _objCharacter.Created
                             || (objLoopImprovement.Condition == "create") != _objCharacter.Created)
                            && objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupKarmaCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                           intUpper,
                                                           objLoopImprovement.Maximum == 0
                                                               ? int.MaxValue
                                                               : objLoopImprovement.Maximum)
                                                       - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                        break;

                                    case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                           intUpper,
                                                           objLoopImprovement.Maximum == 0
                                                               ? int.MaxValue
                                                               : objLoopImprovement.Maximum)
                                                       - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    intCost = (intCost * decMultiplier + decExtra).StandardRound();
                else
                    intCost += decExtra.StandardRound();

                return Math.Max(intCost, 0);
            }
        }

        public int UpgradeKarmaCost
        {
            get
            {
                if (IsDisabled)
                    return -1;
                int intRating = SkillList.Any(x => x.Enabled)
                    ? SkillList.Where(x => x.Enabled).Min(x => x.TotalBaseRating)
                    : 0;
                int intReturn;
                int intOptionsCost;
                if (intRating == 0)
                {
                    intOptionsCost = CharacterObject.Settings.KarmaNewSkillGroup;
                    intReturn = intOptionsCost;
                }
                else if (RatingMaximum > intRating)
                {
                    intOptionsCost = CharacterObject.Settings.KarmaImproveSkillGroup;
                    intReturn = (intRating + 1) * intOptionsCost;
                }
                else
                {
                    return -1;
                }
                
                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                    {
                        if ((objLoopImprovement.Maximum == 0 || intRating + 1 <= objLoopImprovement.Maximum)
                            && objLoopImprovement.Minimum <= intRating + 1 &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition)
                             || (objLoopImprovement.Condition == "career") == _objCharacter.Created
                             || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                else
                    intReturn += decExtra.StandardRound();

                return Math.Max(intReturn, Math.Min(1, intOptionsCost));
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return InternalId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            return obj is SkillGroup objOther && Equals(objOther);
        }

        public bool Equals(SkillGroup other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;
            return InternalId == other.InternalId;
        }

        /// <summary>
        /// List of skills that belong to this skill group.
        /// </summary>
        public IReadOnlyList<Skill> SkillList => _lstAffectedSkills;

        #endregion All the other stuff that is required
    }
}
