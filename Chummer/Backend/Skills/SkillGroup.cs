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
    public class SkillGroup : INotifyPropertyChanged, IHasInternalId, IHasName
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
                return _intSkillFromSp + FreeBase;
            }
            set
            {
                if (BaseUnbroken)
                {
                    int intOld = _intSkillFromSp;

                    //Calculate how far above maximum we are.
                    int intOverMax = (-1) * (RatingMaximum - (value + _intSkillFromKarma + FreeLevels));

                    //reduce value by max or 0
                    //TODO karma from skill, karma other stuff might be reduced
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    _intSkillFromSp = Math.Max(0, value - FreeBase);

                    if (intOld != _intSkillFromSp) OnPropertyChanged();
                }
            }
        }

        public int Karma
        {
            get
            {
                if (IsDisabled)
                    return 0;
                return _intSkillFromKarma + FreeLevels;
            }
            set
            {
                if (KarmaUnbroken)
                {
                    int intOld = _intSkillFromKarma;

                    //Calculate how far above maximum we are.
                    int intOverMax = (-1) * (RatingMaximum - (value + _intSkillFromSp + FreeBase));

                    //reduce value by max or 0
                    //TODO can remove karma from skills
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    _intSkillFromKarma = Math.Max(0, value - FreeLevels);

                    if (intOld != _intSkillFromKarma) OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplifly databinding
        /// </summary>
        public bool BaseUnbroken
        {
            get
            {
                if (IsDisabled || _lstAffectedSkills.Count == 0)
                    return false;
                return _objCharacter.BuildMethodHasSkillPoints && (_objCharacter.Options.StrictSkillGroupsInCreateMode || !_objCharacter.Options.UsePointsOnBrokenGroups) && !_lstAffectedSkills.Any(x => x.Ibase > 0);
            }
        }

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplifly databinding
        /// </summary>
        public bool KarmaUnbroken
        {
            get
            {
                if (IsDisabled || _lstAffectedSkills.Count == 0)
                    return false;
                int high = _lstAffectedSkills.Max(x => x.Ibase);
                bool ret = _lstAffectedSkills.Any(x => x.Ibase + x.Ikarma < high);

                return !ret;
            }
        }

        private bool _blnCachedGroupEnabledIsCached;
        private bool _blnCachedGroupEnabled;
        public bool IsDisabled
        {
            get
            {
                if (!_blnCachedGroupEnabledIsCached)
                {
                    _blnCachedGroupEnabled = _objCharacter.Improvements.Any(x =>
                        ((x.ImproveType == Improvement.ImprovementType.SkillGroupDisable && x.ImprovedName == Name) ||
                        (x.ImproveType == Improvement.ImprovementType.SkillGroupCategoryDisable && GetRelevantSkillCategories.Contains(x.ImprovedName)))
                        && x.Enabled);
                    _blnCachedGroupEnabledIsCached = true;
                }
                return _blnCachedGroupEnabled;
            }
        }

        /// <summary>
        /// Can this skillgroup be increaced in career mode?
        /// </summary>
        public bool CareerIncrease
        {
            get => _blnCareerIncrease;
            set
            {
                if (_blnCareerIncrease != value)
                {
                    _blnCareerIncrease = value;
                    OnPropertyChanged(nameof(CareerIncrease));
                }
            }
        }

        public void DoRefreshCareerIncrease()
        {
            bool blnOldCareerIncrease = CareerIncrease;
            if (_objCharacter.Created)
            {
                if (IsDisabled || _lstAffectedSkills.Count == 0)
                    CareerIncrease = false;
                else
                {
                    int intFirstSkillTotalBaseRating = _lstAffectedSkills[0].TotalBaseRating;
                    if (_lstAffectedSkills.Any(x => x.Specializations.Count != 0 || x.TotalBaseRating != intFirstSkillTotalBaseRating && x.Enabled))
                        CareerIncrease = false;
                    else if (_objCharacter.Improvements.Any(x => ((x.ImproveType == Improvement.ImprovementType.SkillGroupDisable && x.ImprovedName == Name) ||
                                                                  (x.ImproveType == Improvement.ImprovementType.SkillGroupCategoryDisable && GetRelevantSkillCategories.Contains(x.ImprovedName))) && x.Enabled))
                        CareerIncrease = false;
                    else
                        CareerIncrease = _lstAffectedSkills.Max(x => x.TotalBaseRating) < RatingMaximum;
                }
            }
            // CareerIncrease hasn't changed, so we will need to fire off UpgradeToolTip manually
            if (blnOldCareerIncrease == CareerIncrease)
                OnPropertyChanged(nameof(UpgradeToolTip));
        }

        public bool CareerCanIncrease
        {
            get
            {
                if (UpgradeKarmaCost > CharacterObject.Karma) return false;

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

        public int RatingMaximum => (_objCharacter.Created || _objCharacter.IgnoreRules ? 12 : 6);

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
            _strToolTip = string.Empty;
            OnPropertyChanged(nameof(ToolTip));
            skill.PropertyChanged += SkillOnPropertyChanged;
            DoRefreshCareerIncrease();
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

        private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Skill.Base))
            {
                bool blnBaseUnbroken = BaseUnbroken;
                if (_blnBaseBrokenOldValue != blnBaseUnbroken)
                {
                    _blnBaseBrokenOldValue = blnBaseUnbroken;
                    OnPropertyChanged(nameof(BaseUnbroken));
                }

                bool blnKarmaUnbroken = KarmaUnbroken;
                if (!blnKarmaUnbroken && _intSkillFromKarma > 0)
                {
                    _intSkillFromKarma = 0;
                    OnPropertyChanged(nameof(Karma));
                }

                if (_blnKarmaBrokenOldValue != blnKarmaUnbroken)
                {
                    _blnKarmaBrokenOldValue = blnKarmaUnbroken;
                    OnPropertyChanged(nameof(KarmaUnbroken));
                }
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(Skill.Karma))
            {
                bool blnKarmaUnbroken = KarmaUnbroken;
                if (!blnKarmaUnbroken && _intSkillFromKarma > 0)
                {
                    _intSkillFromKarma = 0;
                    OnPropertyChanged(nameof(Karma));
                }
                
                if (_blnKarmaBrokenOldValue != blnKarmaUnbroken)
                {
                    _blnKarmaBrokenOldValue = blnKarmaUnbroken;
                    OnPropertyChanged(nameof(KarmaUnbroken));
                }
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(Skill.TotalBaseRating) || propertyChangedEventArgs.PropertyName == nameof(Skill.Specialization))
                DoRefreshCareerIncrease();
        }

        private bool _blnBaseBrokenOldValue;
        private bool _blnKarmaBrokenOldValue;
        private bool _blnCareerIncrease;
        private readonly List<Skill> _lstAffectedSkills = new List<Skill>();
        private string _strGroupName;
        private readonly Character _objCharacter;

        public SkillGroup(Character objCharacter, string strGroupName = "")
        {
            _objCharacter = objCharacter;
            _strGroupName = strGroupName;
            _blnBaseBrokenOldValue = BaseUnbroken;
            
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
                    _intCachedFreeBase = int.MinValue;
                    _intCachedFreeLevels = int.MinValue;
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
                return SkillList.Min(x => x.TotalBaseRating).ToString();
            }
        }

        private string _strToolTip = string.Empty;
        public string ToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(_strToolTip))
                    _strToolTip = LanguageManager.GetString("Tip_SkillGroup_Skills", GlobalOptions.Language) + ' ' + string.Join(", ", _lstAffectedSkills.Select(x => x.DisplayNameMethod(GlobalOptions.Language)));
                return _strToolTip;
            }
        }

        public string UpgradeToolTip
        {
            get { return string.Format(LanguageManager.GetString("Tip_ImproveItem", GlobalOptions.Language), SkillList.Select(x => x.TotalBaseRating).DefaultIfEmpty().Min() + 1, UpgradeKarmaCost); }
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
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(FreeLevels))
                _intCachedFreeLevels = int.MinValue;
            else if (propertyName == nameof(FreeBase))
                _intCachedFreeBase = int.MinValue;
            else if (propertyName == nameof(IsDisabled))
            {
                _blnCachedGroupEnabledIsCached = false;
                DoRefreshCareerIncrease();
            }
            else if (propertyName == nameof(CareerIncrease))
                OnPropertyChanged(nameof(CareerCanIncrease));
            else if (propertyName == nameof(UpgradeKarmaCost))
            {
                OnPropertyChanged(nameof(CareerCanIncrease));
                OnPropertyChanged(nameof(UpgradeToolTip));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Character_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CareerCanIncrease));
        }

        public int CurrentSpCost()
        {
            int intReturn = _intSkillFromSp;
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

        public int CurrentKarmaCost()
        {
            if (_intSkillFromKarma == 0) return 0;

            int intUpper = _lstAffectedSkills.Min(x => x.TotalBaseRating);
            int intLower = intUpper - _intSkillFromKarma;

            int intCost = intUpper*(intUpper + 1);
            intCost -= intLower*(intLower + 1);
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

        public int UpgradeKarmaCost
        {
            get
            {
                if (IsDisabled)
                    return -1;
                int intRating = SkillList.Select(x => x.TotalBaseRating).DefaultIfEmpty().Min();
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
                    if ((objLoopImprovement.Maximum == 0 || intRating <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intRating &&
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
