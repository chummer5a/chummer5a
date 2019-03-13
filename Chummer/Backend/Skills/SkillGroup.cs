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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Annotations;
using System.Globalization;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strGroupName} {_intSkillFromSp} {_intSkillFromKarma}")]
    public class SkillGroup : INotifyMultiplePropertyChanged, IHasInternalId, IHasName
    {
        #region Core calculations
        private int _intSkillFromSp;
        private int _intSkillFromKarma;

        public int Base
        {
            get
            {
                if (IsDisabled)
                    return 0;
                return Math.Min(BasePoints + FreeBase, RatingMaximum);
            }
            set
            {
                if (BaseUnbroken)
                {
                    //Calculate how far above maximum we are.
                    int intOverMax = (value + KarmaPoints + FreeLevels) - RatingMaximum;

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    BasePoints = Math.Max(0, value - FreeBase);
                    foreach (Skill skill in _lstAffectedSkills)
                    {
                        //To trigger new calculation of skill.KarmaPoints
                        skill.Karma = skill.Karma;
                    }
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
                if (IsDisabled)
                    return 0;
                return Math.Min(KarmaPoints + FreeLevels, RatingMaximum);
            }
            set
            {
                if (KarmaUnbroken)
                {
                    //Calculate how far above maximum we are.
                    int intOverMax = value + BasePoints + FreeBase - RatingMaximum;

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    KarmaPoints = Math.Max(0, value - FreeLevels);
                    foreach (Skill skill in _lstAffectedSkills)
                    {
                        //To trigger new calculation of skill.KarmaPoints
                        skill.Karma = skill.Karma;
                    }
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
                if (_intSkillFromSp != value)
                {
                    _intSkillFromSp = value;
                    OnPropertyChanged();
                }
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
                if (_intSkillFromKarma != value)
                {
                    _intSkillFromKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedBaseUnbroken = -1;

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplifly databinding
        /// </summary>
        public bool BaseUnbroken
        {
            get
            {
                if (_intCachedBaseUnbroken < 0)
                {
                    if (IsDisabled || SkillList.Count == 0)
                        _intCachedBaseUnbroken = 0;
                    else
                    {
                        _intCachedBaseUnbroken = _objCharacter.BuildMethodHasSkillPoints && (_objCharacter.Options.StrictSkillGroupsInCreateMode || !_objCharacter.Options.UsePointsOnBrokenGroups) &&
                            !SkillList.Any(x => x.BasePoints + x.FreeBase > 0) ? 1 : 0;
                    }
                }
                return _intCachedBaseUnbroken > 0;
            }
        }

        private int _intCachedKarmaUnbroken = -1;

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplifly databinding
        /// </summary>
        public bool KarmaUnbroken
        {
            get
            {
                if (_intCachedKarmaUnbroken < 0)
                {
                    if (IsDisabled || SkillList.Count == 0)
                        _intCachedKarmaUnbroken = 0;
                    else
                    {
                        int high = SkillList.Max(x => x.BasePoints + x.FreeBase);

                        _intCachedKarmaUnbroken = SkillList.All(x => x.BasePoints + x.FreeBase + x.KarmaPoints + x.FreeKarma >= high) ? 1 : 0;
                    }
                }
                return _intCachedKarmaUnbroken > 0;
            }
        }

        private int _intCachedIsDisabled = -1;

        public bool IsDisabled
        {
            get
            {
                if (_intCachedIsDisabled < 0)
                {
                    _intCachedIsDisabled = _objCharacter.Improvements.Any(x =>
                        ((x.ImproveType == Improvement.ImprovementType.SkillGroupDisable && x.ImprovedName == Name) ||
                        (x.ImproveType == Improvement.ImprovementType.SkillGroupCategoryDisable && GetRelevantSkillCategories.Contains(x.ImprovedName)))
                        && x.Enabled) ? 1 : 0;
                }
                return _intCachedIsDisabled > 0;
            }
        }

        private int _intCachedCareerIncrease = -1;

        /// <summary>
        /// Can this skillgroup be increaced in career mode?
        /// </summary>
        public bool CareerIncrease
        {
            get
            {
                if (_intCachedCareerIncrease < 0)
                {
                    if (_objCharacter.Created)
                    {
                        if (IsDisabled || _lstAffectedSkills.Count == 0)
                            _intCachedCareerIncrease = 0;
                        else
                        {
                            var firstOrDefault = _lstAffectedSkills.FirstOrDefault(x => x.Enabled);
                            if (firstOrDefault != null)
                            {
                                int intFirstSkillTotalBaseRating = firstOrDefault.TotalBaseRating;
                                if (_lstAffectedSkills.Any(x => x.Specializations.Count != 0 || x.TotalBaseRating != intFirstSkillTotalBaseRating && x.Enabled))
                                    _intCachedCareerIncrease = 0;
                                else if (_objCharacter.Improvements.Any(x => ((x.ImproveType == Improvement.ImprovementType.SkillGroupDisable && x.ImprovedName == Name) ||
                                                                              (x.ImproveType == Improvement.ImprovementType.SkillGroupCategoryDisable && GetRelevantSkillCategories.Contains(x.ImprovedName))) && x.Enabled))
                                    _intCachedCareerIncrease = 0;
                                else
                                    _intCachedCareerIncrease = _lstAffectedSkills.Max(x => x.TotalBaseRating) < RatingMaximum ? 1 : 0;
                            }
                        }

                        if (_intCachedCareerIncrease > 0)
                        {
                            Skill objSkill = _lstAffectedSkills.FirstOrDefault(x => x.Enabled);
                            if (objSkill != null)
                            {
                                foreach (Skill objDisabledSkill in _lstAffectedSkills.Where(x => !x.Enabled))
                                {
                                    objDisabledSkill.Karma = objSkill.Karma;
                                    objDisabledSkill.Base = objSkill.Base;
                                }
                            }
                        }
                    }
                }

                return _intCachedCareerIncrease > 0;
            }
        }
        
        public bool CareerCanIncrease
        {
            get
            {
                if (UpgradeKarmaCost > CharacterObject.Karma)
                    return false;

                return CareerIncrease;
            }
        }

        public int Rating => Karma + Base;

        private int _intCachedFreeBase = int.MinValue;
        public int FreeBase
        {
            get
            {
                if (_intCachedFreeBase != int.MinValue)
                    return _intCachedFreeBase;

                return _intCachedFreeBase = string.IsNullOrEmpty(Name) ? 0 : ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupBase, false, Name);
            }
        }

        private int _intCachedFreeLevels = int.MinValue;
        public int FreeLevels
        {
            get
            {
                if (_intCachedFreeLevels != int.MinValue)
                    return _intCachedFreeLevels;

                return _intCachedFreeLevels = string.IsNullOrEmpty(Name) ? 0 : ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupLevel, false, Name);
            }
        }

        public int RatingMaximum => (_objCharacter.Created || _objCharacter.IgnoreRules ? 12 : _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule ? 7 : 6);

        public void Upgrade()
        {
            if (CharacterObject.Created)
            {
                if (!CareerIncrease)
                    return;

                int intPrice = UpgradeKarmaCost;

                //If data file contains {4} this crashes but...
                string strUpgradetext =
                    $"{LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)} {DisplayName} {Rating} -> {(Rating + 1)}";

                ExpenseLogEntry objEntry = new ExpenseLogEntry(_objCharacter);
                objEntry.Create(intPrice * -1, strUpgradetext, ExpenseType.Karma, DateTime.Now);
                objEntry.Undo = new ExpenseUndo().CreateKarma(Rating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, Name);

                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                CharacterObject.Karma -= intPrice;
            }

            Karma += 1;
        }

        #endregion

        #region All the other stuff that is required
        public static SkillGroup Get(Skill objSkill)
        {
            if (objSkill.SkillGroupObject != null)
                return objSkill.SkillGroupObject;

            if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                return null;

            foreach (SkillGroup objSkillGroup in objSkill.CharacterObject.SkillsSection.SkillGroups)
            {
                if (objSkillGroup.Name == objSkill.SkillGroup)
                {
                    if(!objSkillGroup.SkillList.Contains(objSkill))
                        objSkillGroup.Add(objSkill);
                    return objSkillGroup;
                }
            }

            SkillGroup objNewGroup = new SkillGroup(objSkill.CharacterObject, objSkill.SkillGroup);
            objNewGroup.Add(objSkill);
            objSkill.CharacterObject.SkillsSection.SkillGroups.MergeInto(objNewGroup, (l, r) => string.Compare(l.DisplayName, r.DisplayName, StringComparison.Ordinal),
                (l, r) => { foreach (Skill x in r.SkillList.Where(y => !l.SkillList.Contains(y))) l.SkillList.Add(x); });

            return objNewGroup;
        }

        private void Add(Skill skill)
        {
            _lstAffectedSkills.Add(skill);
            skill.PropertyChanged += SkillOnPropertyChanged;
            OnPropertyChanged(nameof(SkillList));
        }

        internal void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("group");

            writer.WriteElementString("karma", _intSkillFromKarma.ToString());
            writer.WriteElementString("base", _intSkillFromSp.ToString());
            writer.WriteElementString("id", _guidId.ToString("D"));
            writer.WriteElementString("name", _strGroupName);

            writer.WriteEndElement();
        }

        internal void Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("skillgroup");

            objWriter.WriteElementString("name", DisplayNameMethod(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratingmax", RatingMaximum.ToString(objCulture));
            objWriter.WriteElementString("base", Base.ToString(objCulture));
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));

            objWriter.WriteEndElement();
        }

        public void Load(XmlNode xmlNode)
        {
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid g))
                _guidId = g;
            xmlNode.TryGetStringFieldQuickly("name", ref _strGroupName);
            xmlNode.TryGetInt32FieldQuickly("karma", ref _intSkillFromKarma);
            xmlNode.TryGetInt32FieldQuickly("base", ref _intSkillFromSp);
        }

        public void LoadFromHeroLab(XmlNode xmlNode)
        {
            string strTemp = xmlNode.SelectSingleNode("@name")?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
                _strGroupName = strTemp.TrimEndOnce("Group").Trim();
            strTemp = xmlNode.SelectSingleNode("@base")?.InnerText;
            if (!string.IsNullOrEmpty(strTemp) && int.TryParse(strTemp, out int intTemp))
                _intSkillFromKarma = intTemp;
        }

        private static readonly DependancyGraph<string> SkillGroupDependencyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(DisplayRating),
                    new DependancyGraphNode<string>(nameof(SkillList)),
                    new DependancyGraphNode<string>(nameof(CareerIncrease),
                        new DependancyGraphNode<string>(nameof(SkillList)),
                        new DependancyGraphNode<string>(nameof(RatingMaximum)),
                        new DependancyGraphNode<string>(nameof(IsDisabled),
                            new DependancyGraphNode<string>(nameof(Name))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(Rating),
                        new DependancyGraphNode<string>(nameof(Karma),
                            new DependancyGraphNode<string>(nameof(IsDisabled)),
                            new DependancyGraphNode<string>(nameof(RatingMaximum)),
                            new DependancyGraphNode<string>(nameof(FreeLevels),
                                new DependancyGraphNode<string>(nameof(Name))
                            ),
                            new DependancyGraphNode<string>(nameof(KarmaPoints)),
                            new DependancyGraphNode<string>(nameof(KarmaUnbroken),
                                new DependancyGraphNode<string>(nameof(IsDisabled)),
                                new DependancyGraphNode<string>(nameof(SkillList))
                            )
                        ),
                        new DependancyGraphNode<string>(nameof(Base),
                            new DependancyGraphNode<string>(nameof(IsDisabled)),
                            new DependancyGraphNode<string>(nameof(RatingMaximum)),
                            new DependancyGraphNode<string>(nameof(FreeBase),
                                new DependancyGraphNode<string>(nameof(Name))
                            ),
                            new DependancyGraphNode<string>(nameof(BasePoints))
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(UpgradeToolTip),
                    new DependancyGraphNode<string>(nameof(SkillList)),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost),
                        new DependancyGraphNode<string>(nameof(SkillList)),
                        new DependancyGraphNode<string>(nameof(IsDisabled)),
                        new DependancyGraphNode<string>(nameof(Rating)),
                        new DependancyGraphNode<string>(nameof(Name))
                    )
                ),
                new DependancyGraphNode<string>(nameof(CareerCanIncrease),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost)),
                    new DependancyGraphNode<string>(nameof(CareerIncrease))
                ),
                new DependancyGraphNode<string>(nameof(BaseUnbroken),
                    new DependancyGraphNode<string>(nameof(IsDisabled)),
                    new DependancyGraphNode<string>(nameof(SkillList))
                ),
                new DependancyGraphNode<string>(nameof(ToolTip),
                    new DependancyGraphNode<string>(nameof(SkillList))
                ),
                new DependancyGraphNode<string>(nameof(DisplayName),
                    new DependancyGraphNode<string>(nameof(Name))
                ),
                new DependancyGraphNode<string>(nameof(CurrentSpCost),
                    new DependancyGraphNode<string>(nameof(BasePoints)),
                    new DependancyGraphNode<string>(nameof(Name))
                ),
                new DependancyGraphNode<string>(nameof(CurrentKarmaCost),
                    new DependancyGraphNode<string>(nameof(KarmaPoints)),
                    new DependancyGraphNode<string>(nameof(SkillList))
                )
            );

        private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Skill.BasePoints) ||
                e.PropertyName == nameof(Skill.FreeBase))
            {
                OnMultiplePropertyChanged(nameof(BaseUnbroken), nameof(KarmaUnbroken));
            }
            else if (e.PropertyName == nameof(Skill.KarmaPoints) ||
                e.PropertyName == nameof(Skill.FreeKarma))
            {
                OnPropertyChanged(nameof(KarmaUnbroken));
            }
            else if (e.PropertyName == nameof(Skill.Specializations))
                OnPropertyChanged(nameof(CareerIncrease));
            else if (e.PropertyName == nameof(Skill.TotalBaseRating) ||
                e.PropertyName == nameof(Skill.Enabled))
                OnMultiplePropertyChanged(nameof(CareerIncrease),
                                          nameof(DisplayRating),
                                          nameof(UpgradeToolTip),
                                          nameof(CurrentKarmaCost),
                                          nameof(UpgradeKarmaCost));
        }
        
        private readonly List<Skill> _lstAffectedSkills = new List<Skill>();
        private string _strGroupName;
        private readonly Character _objCharacter;

        public SkillGroup(Character objCharacter, string strGroupName = "")
        {
            _objCharacter = objCharacter;
            _strGroupName = strGroupName;

            _objCharacter.PropertyChanged += Character_PropertyChanged;
        }

        public void UnbindSkillGroup()
        {
            _objCharacter.PropertyChanged -= Character_PropertyChanged;
            foreach (Skill objSkill in _lstAffectedSkills)
                objSkill.PropertyChanged -= SkillOnPropertyChanged;
        }

        public Character CharacterObject => _objCharacter;

        public string Name
        {
            get => _strGroupName;
            set
            {
                if (value != _strGroupName)
                {
                    _strGroupName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string DisplayName => DisplayNameMethod(GlobalOptions.Language);

        public string DisplayNameMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;
            return XmlManager.Load("skills.xml", strLanguage).SelectSingleNode("/chummer/skillgroups/name[. = \"" + Name + "\"]/@translate")?.InnerText ?? Name;
        }

        public string DisplayRating
        {
            get
            {
                if (_objCharacter.Created && !CareerIncrease)
                {
                    return LanguageManager.GetString("Label_SkillGroup_Broken", GlobalOptions.Language);
                }

                return SkillList.Any(x => x.Enabled && x.TotalBaseRating > 0) ? SkillList.Where(x => x.Enabled).Min(x => x.TotalBaseRating).ToString() : 0.ToString();
            }
        }

        private string _strToolTip = string.Empty;
        public string ToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(_strToolTip))
                {
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    _strToolTip = LanguageManager.GetString("Tip_SkillGroup_Skills", GlobalOptions.Language) + strSpaceCharacter + string.Join(',' + strSpaceCharacter, _lstAffectedSkills.Select(x => x.DisplayNameMethod(GlobalOptions.Language)));
                }
                return _strToolTip;
            }
        }

        public string UpgradeToolTip
        {
            get { return string.Format(LanguageManager.GetString("Tip_ImproveItem", GlobalOptions.Language), SkillList.Where(x => x.Enabled).Select(x => x.TotalBaseRating).DefaultIfEmpty().Min() + 1, UpgradeKarmaCost); }
        }

        private Guid _guidId = Guid.NewGuid();
        public Guid Id => _guidId;
        public string InternalId => _guidId.ToString("D");

        #region HasWhateverSkills
        public bool HasCombatSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Combat Active"); }
        }

        public bool HasPhysicalSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Physical Active"); }
        }

        public bool HasSocialSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Social Active"); }
        }

        public bool HasTechnicalSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Technical Active"); }
        }

        public bool HasVehicleSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Vehicle Active"); }
        }

        public bool HasMagicalSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Magical Active"); }
        }

        public bool HasResonanceSkills
        {
            get { return _lstAffectedSkills.Any(x => x.SkillCategory == "Resonance Active"); }
        }

        public IEnumerable<string> GetRelevantSkillCategories
        {
            get { return _lstAffectedSkills.Select(x => x.SkillCategory).Distinct(); }
        }
        #endregion

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
                    lstNamesOfChangedProperties = SkillGroupDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in SkillGroupDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(FreeBase)))
                _intCachedFreeBase = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(FreeLevels)))
                _intCachedFreeLevels = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(IsDisabled)))
                _intCachedIsDisabled = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(CareerIncrease)))
                _intCachedCareerIncrease = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(KarmaUnbroken)))
                _intCachedKarmaUnbroken = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(BaseUnbroken)))
                _intCachedBaseUnbroken = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(ToolTip)))
                _strToolTip = string.Empty;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private void Character_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.Karma))
                OnPropertyChanged(nameof(CareerCanIncrease));
            else if (e.PropertyName == nameof(Character.BuildMethodHasSkillPoints))
                OnPropertyChanged(nameof(BaseUnbroken));
        }

        public int CurrentSpCost
        {
            get
            {
                int intReturn = BasePoints;
                int intValue = intReturn;

                List<string> lstRelevantCategories = GetRelevantSkillCategories.ToList();
                decimal decMultiplier = 1.0m;
                int intExtra = 0;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intValue <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupPointCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intValue, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryPointCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intValue, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));
                intReturn += intExtra;

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
                intCost /= 2; //We get sqre, need triangle

                if (intCost == 1)
                    intCost *= _objCharacter.Options.KarmaNewSkillGroup;
                else
                    intCost *= _objCharacter.Options.KarmaImproveSkillGroup;

                List<string> lstRelevantCategories = GetRelevantSkillCategories.ToList();
                decimal decMultiplier = 1.0m;
                int intExtra = 0;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intLower &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupKarmaCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intUpper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryKarmaCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intUpper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intCost = decimal.ToInt32(decimal.Ceiling(intCost * decMultiplier));
                intCost += intExtra;

                return Math.Max(intCost, 0);
            }
        }

        public int UpgradeKarmaCost
        {
            get
            {
                if (IsDisabled)
                    return -1;
                int intRating = SkillList.Where(x => x.Enabled).Select(x => x.TotalBaseRating).DefaultIfEmpty().Min();
                int intReturn;
                int intOptionsCost;
                if (intRating == 0)
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewSkillGroup;
                    intReturn = intOptionsCost;
                }
                else if (RatingMaximum > intRating)
                {
                    intOptionsCost = CharacterObject.Options.KarmaImproveSkillGroup;
                    intReturn = (intRating + 1) * intOptionsCost;
                }
                else
                {
                    return -1;
                }

                List<string> lstRelevantCategories = GetRelevantSkillCategories.ToList();
                decimal decMultiplier = 1.0m;
                int intExtra = 0;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intRating + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intRating + 1 &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                        objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupKarmaCost)
                                intExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryKarmaCost)
                                intExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));
                intReturn += intExtra;

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

        /// <summary>
        /// List of skills that belong to this skill group.
        /// </summary>
        public IList<Skill> SkillList => _lstAffectedSkills;

        #endregion
    }
}
