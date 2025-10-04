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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Skills
{
    public class KnowledgeSkill : Skill
    {
        public override bool IsKnowledgeSkill => true;


        /// <summary>
        /// The karma cost to improve this skill by one rating
        /// </summary>
        public override int KarmaImproveSkillCost => CharacterObjectSettings.KarmaImproveKnowledgeSkill;

        /// <summary>
        /// The karma cost to buy the first rating of this skill
        /// </summary>
        public override int KarmaNewSkillCost => CharacterObjectSettings.KarmaNewKnowledgeSkill;

        /// <summary>
        /// The karma cost for a specialization in this skill
        /// </summary>
        public override int KarmaSpecializationCost => CharacterObjectSettings.KarmaKnowledgeSpecialization;

        /// <summary>
        /// The karma cost to improve this skill by one rating (async)
        /// </summary>
        public override async Task<int> GetKarmaImproveSkillCostAsync(CancellationToken token = default)
        {
            return await CharacterObjectSettings.GetKarmaImproveKnowledgeSkillAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// The karma cost to buy the first rating of this skill (async)
        /// </summary>
        public override async Task<int> GetKarmaNewSkillCostAsync(CancellationToken token = default)
        {
            return await CharacterObjectSettings.GetKarmaNewKnowledgeSkillAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// The karma cost for a specialization in this skill (async)
        /// </summary>
        public override async Task<int> GetKarmaSpecializationCostAsync(CancellationToken token = default)
        {
            return await CharacterObjectSettings.GetKarmaKnowledgeSpecializationAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// The appropriate karma cost improvement type for this skill
        /// </summary>
        public override Improvement.ImprovementType KarmaCostImprovementType => Improvement.ImprovementType.KnowledgeSkillKarmaCost;

        /// <summary>
        /// The appropriate karma cost multiplier improvement type for this skill
        /// </summary>
        public override Improvement.ImprovementType KarmaCostMultiplierImprovementType => Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier;

        /// <summary>
        /// The appropriate minimum cost override improvement type for this skill
        /// </summary>
        public override Improvement.ImprovementType KarmaCostMinimumImprovementType => Improvement.ImprovementType.KnowledgeSkillKarmaCostMinimum;

        public override bool AllowDelete
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return (!ForcedName || FreeBase + FreeKarma + RatingModifiers(Attribute) <= 0) && !IsNativeLanguage;
            }
        }

        public override async Task<bool> GetAllowDeleteAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (!ForcedName || await GetFreeBaseAsync(token).ConfigureAwait(false)
                           + await GetFreeKarmaAsync(token).ConfigureAwait(false)
                           + await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false) <= 0)
                       && !await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool AllowNameChange
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !ForcedName && (AllowUpgrade || IsNativeLanguage) &&
                           (!CharacterObject.Created || (Karma == 0 && Base == 0 && !IsNativeLanguage));
            }
        }

        public override async Task<bool> GetAllowNameChangeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnIsNativeLanguage = await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
                return !ForcedName && (blnIsNativeLanguage || await GetAllowUpgradeAsync(token).ConfigureAwait(false))
                                   &&
                                   (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                                    || (await GetKarmaAsync(token).ConfigureAwait(false) == 0
                                        && await GetBaseAsync(token).ConfigureAwait(false) == 0
                                        && !blnIsNativeLanguage));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool AllowTypeChange
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return (AllowNameChange || string.IsNullOrWhiteSpace(Type)) && !IsNativeLanguage;
            }
        }

        public override async Task<bool> GetAllowTypeChangeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await GetAllowNameChangeAsync(token).ConfigureAwait(false)
                        || string.IsNullOrWhiteSpace(await GetTypeAsync(token).ConfigureAwait(false)))
                       && !await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool CanHaveSpecs
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return AllowUpgrade && base.CanHaveSpecs;
            }
        }

        public override async Task<bool> GetCanHaveSpecsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetAllowUpgradeAsync(token).ConfigureAwait(false) && await base.GetCanHaveSpecsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _strType = string.Empty;

        private int _intIsNativeLanguage;

        public bool ForcedName { get; private set; }

        public KnowledgeSkill(Character objCharacter, bool blnSetProperties = true) : base(objCharacter)
        {
            if (blnSetProperties)
                DefaultAttribute = "LOG";
            IsLoading = false;
        }

        public KnowledgeSkill(Character objCharacter, string strForcedName, bool blnAllowUpgrade) : this(objCharacter)
        {
            WritableName = strForcedName;
            ForcedName = true;
            _blnAllowUpgrade = blnAllowUpgrade;
            IsLoading = false;
        }

        public static async Task<KnowledgeSkill> NewAsync(Character objCharacter, string strForcedName,
            bool blnAllowUpgrade, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            KnowledgeSkill objReturn = new KnowledgeSkill(objCharacter, false);
            try
            {
                await objReturn.SetDefaultAttributeAsync("LOG", token).ConfigureAwait(false);
                await objReturn.SetWritableNameAsync(strForcedName, token).ConfigureAwait(false);
                objReturn.ForcedName = true;
                objReturn._blnAllowUpgrade = blnAllowUpgrade;
            }
            catch
            {
                await objReturn.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }

            return objReturn;
        }

        private bool _blnAllowUpgrade = true;

        /// <summary>
        /// Is the skill allowed to be upgraded through karma or points?
        /// </summary>
        public bool AllowUpgrade
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowUpgrade && !IsNativeLanguage;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnAllowUpgrade == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowUpgrade == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowUpgrade = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Is the skill allowed to be upgraded through karma or points?
        /// </summary>
        public async Task<bool> GetAllowUpgradeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowUpgrade && !await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string WritableName
        {
            get => CurrentDisplayName;
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (ForcedName)
                        return;
                    if (string.Equals(CurrentDisplayName, value, StringComparison.Ordinal))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (ForcedName)
                        return;
                    if (string.Equals(CurrentDisplayName, value, StringComparison.Ordinal))
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        LoadSkillFromData(value);
                        Name = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public Task<string> GetWritableNameAsync(CancellationToken token = default) => GetCurrentDisplayNameAsync(token);

        public async Task SetWritableNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (ForcedName)
                    return;
                if (string.Equals(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), value,
                        StringComparison.Ordinal))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (ForcedName)
                    return;
                if (string.Equals(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), value,
                        StringComparison.Ordinal))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await LoadSkillFromDataAsync(value, token).ConfigureAwait(false);
                    await SetNameAsync(value, token).ConfigureAwait(false);
                    await OnPropertyChangedAsync(nameof(WritableName), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void LoadSkillFromData(string strInputSkillName)
        {
            using (LockObject.EnterWriteLock())
            {
                string strSkillName = GetSkillNameFromData(strInputSkillName);
                XPathNavigator xmlSkillNode = CharacterObject.LoadDataXPath("skills.xml")
                    .TryGetNodeByNameOrId("/chummer/knowledgeskills/skill", strSkillName);

                if (xmlSkillNode == null)
                {
                    SkillId = Guid.Empty;
                    return;
                }

                SkillId = xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guidTemp)
                    ? guidTemp
                    : Guid.Empty;

                string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category")?.Value;

                if (!string.IsNullOrEmpty(strCategory))
                {
                    Type = strCategory;
                }

                string strAttribute = xmlSkillNode.SelectSingleNodeAndCacheExpression("attribute")?.Value;

                if (!string.IsNullOrEmpty(strAttribute))
                {
                    DefaultAttribute = CharacterObject.GetAttribute(strAttribute) != null ? strAttribute : "LOG";
                }
            }
        }

        private async Task LoadSkillFromDataAsync(string strInputSkillName, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strSkillName = await GetSkillNameFromDataAsync(strInputSkillName, token).ConfigureAwait(false);
                XPathNavigator xmlSkillNode = (await CharacterObject.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                                                             .SelectSingleNode(
                                                                 "/chummer/knowledgeskills/skill[name = "
                                                                 + strSkillName.CleanXPath() + "]");

                if (xmlSkillNode == null)
                {
                    await SetSkillIdAsync(Guid.Empty, token).ConfigureAwait(false);
                    return;
                }

                await SetSkillIdAsync(xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guidTemp)
                                          ? guidTemp
                                          : Guid.Empty, token).ConfigureAwait(false);

                string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category", token)?.Value;

                if (!string.IsNullOrEmpty(strCategory))
                {
                    await SetTypeAsync(strCategory, token).ConfigureAwait(false);
                }

                string strAttribute = xmlSkillNode.SelectSingleNodeAndCacheExpression("attribute", token)?.Value;

                if (!string.IsNullOrEmpty(strAttribute))
                {
                    await SetDefaultAttributeAsync(
                        await CharacterObject.GetAttributeAsync(strAttribute, token: token).ConfigureAwait(false) != null
                            ? strAttribute
                            : "LOG", token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string GetSkillNameFromData(string strInputSkillName)
        {
            if (GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return strInputSkillName;
            }

            using (CharacterObject.LockObject.EnterReadLock())
            {
                XPathNavigator xmlSkillTranslationNode = CharacterObject.LoadDataXPath("skills.xml")
                    .SelectSingleNode("/chummer/knowledgeskills/skill[translate = " + strInputSkillName.CleanXPath() +
                                      "]");

                if (xmlSkillTranslationNode == null)
                {
                    return CharacterObject.ReverseTranslateExtra(strInputSkillName, GlobalSettings.Language,
                        "skills.xml");
                }

                return xmlSkillTranslationNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? strInputSkillName;
            }
        }

        private async Task<string> GetSkillNameFromDataAsync(string strInputSkillName, CancellationToken token = default)
        {
            if (GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return strInputSkillName;
            }

            IAsyncDisposable objLocker = await CharacterObject.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator xmlSkillTranslationNode = (await CharacterObject.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                                                                        .SelectSingleNode("/chummer/knowledgeskills/skill[translate = " + strInputSkillName.CleanXPath() +
                                                                            "]");

                if (xmlSkillTranslationNode == null)
                {
                    return await CharacterObject.ReverseTranslateExtraAsync(strInputSkillName, GlobalSettings.Language,
                                                                            "skills.xml", token).ConfigureAwait(false);
                }

                return xmlSkillTranslationNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? strInputSkillName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override string SkillCategory => Type;

        // ReSharper disable once InconsistentNaming
        private int _intCachedCyberwareRating = int.MinValue;

        protected override void ResetCachedCyberwareRating()
        {
            using (_objCachedCyberwareRatingLock.EnterWriteLock())
                _intCachedCyberwareRating = int.MinValue;
        }

        protected override async Task ResetCachedCyberwareRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedCyberwareRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedCyberwareRating = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The attributeValue this skill have from Skilljacks + Knowsoft
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public override int CyberwareRating
        {
            get
            {
                using (_objCachedCyberwareRatingLock.EnterReadLock())
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;
                }

                using (_objCachedCyberwareRatingLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;
                    using (_objCachedCyberwareRatingLock.EnterWriteLock())
                    {
                        string strTranslatedName = CurrentDisplayName;
                        int intMaxHardwire = -1;
                        foreach (Improvement objImprovement in ImprovementManager
                                     .GetCachedImprovementListForValueOf(
                                         CharacterObject, Improvement.ImprovementType.Hardwire,
                                         DictionaryKey)
                                     .Concat(ImprovementManager.GetCachedImprovementListForValueOf(
                                         CharacterObject,
                                         Improvement.ImprovementType.Hardwire,
                                         strTranslatedName)))
                        {
                            intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                        }

                        if (intMaxHardwire >= 0)
                        {
                            return _intCachedCyberwareRating = intMaxHardwire;
                        }

                        int intMaxSkillsoftRating = ImprovementManager
                            .ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess).StandardRound();
                        if (intMaxSkillsoftRating <= 0)
                            return _intCachedCyberwareRating = 0;
                        int intMax = 0;
                        foreach (Improvement objSkillsoftImprovement in ImprovementManager
                                     .GetCachedImprovementListForValueOf(
                                         CharacterObject, Improvement.ImprovementType.Skillsoft, InternalId))
                        {
                            intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                        }

                        return _intCachedCyberwareRating = Math.Min(intMax, intMaxSkillsoftRating);
                    }
                }
            }
        }

        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public override async Task<int> GetCyberwareRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedCyberwareRatingLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedCyberwareRatingLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                IAsyncDisposable objLocker2 = await _objCachedCyberwareRatingLock.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strTranslatedName = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                    int intMaxHardwire = -1;
                    foreach (Improvement objImprovement in (await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     CharacterObject, Improvement.ImprovementType.Hardwire,
                                     await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                 .ConfigureAwait(false))
                             .Concat(await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                 CharacterObject,
                                 Improvement.ImprovementType.Hardwire,
                                 strTranslatedName, token: token).ConfigureAwait(false)))
                    {
                        intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                    }

                    if (intMaxHardwire >= 0)
                    {
                        return _intCachedCyberwareRating = intMaxHardwire;
                    }

                    int intMaxSkillsoftRating =
                        (await ImprovementManager
                            .ValueOfAsync(CharacterObject, Improvement.ImprovementType.SkillsoftAccess,
                                token: token)
                            .ConfigureAwait(false)).StandardRound();
                    if (intMaxSkillsoftRating <= 0)
                        return _intCachedCyberwareRating = 0;
                    int intMax = 0;
                    foreach (Improvement objSkillsoftImprovement in await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     CharacterObject, Improvement.ImprovementType.Skillsoft, InternalId,
                                     token: token)
                                 .ConfigureAwait(false))
                    {
                        intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                    }

                    return _intCachedCyberwareRating = Math.Min(intMax, intMaxSkillsoftRating);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override string DisplaySpecialization(string strLanguage)
        {
            using (LockObject.EnterReadLock())
                return IsNativeLanguage ? string.Empty : base.DisplaySpecialization(strLanguage);
        }

        public override async Task<string> DisplaySpecializationAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetIsNativeLanguageAsync(token).ConfigureAwait(false)
                    ? string.Empty
                    : await base.DisplaySpecializationAsync(strLanguage, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Type
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strType == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        // Interlocked guarantees thread safety here without write lock
                        string strOldType = Interlocked.Exchange(ref _strType, value);
                        if (strOldType == value)
                            return;
                        string strNewAttributeValue = string.Empty;
                        bool blnSetDefaultAttribute =
                            CharacterObject?.SkillsSection?.KnowledgeSkillCategoriesMap.TryGetValue(value,
                                out strNewAttributeValue) ?? false;
                        bool blnUnsetNativeLanguage = value != "Language" && strOldType == "Language";
                        if (blnSetDefaultAttribute || blnUnsetNativeLanguage)
                        {
                            bool blnTemp1 = false;
                            bool blnTemp2 = false;
                            if (blnSetDefaultAttribute && InterlockExchangeDefaultAttribute(strNewAttributeValue) !=
                                strNewAttributeValue)
                            {
                                if (IsLoading)
                                    RecacheAttribute();
                                else
                                    blnTemp1 = true;
                            }

                            if (blnUnsetNativeLanguage && Interlocked.Exchange(ref _intIsNativeLanguage, 0) == 0)
                            {
                                blnTemp2 = true;
                            }

                            if (blnTemp1)
                            {
                                if (blnTemp2)
                                    this.OnMultiplePropertyChanged(nameof(Type), nameof(DefaultAttribute),
                                        nameof(IsNativeLanguage));
                                else
                                    this.OnMultiplePropertyChanged(nameof(Type), nameof(DefaultAttribute));
                            }
                            else if (blnTemp2)
                                this.OnMultiplePropertyChanged(nameof(Type), nameof(IsNativeLanguage));
                            else
                                OnPropertyChanged();
                        }
                        else
                            OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<string> GetTypeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetTypeAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strType == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Interlocked guarantees thread safety here without write lock
                    string strOldType = Interlocked.Exchange(ref _strType, value);
                    if (strOldType == value)
                        return;
                    string strNewAttributeValue = string.Empty;
                    SkillsSection objSkillsSection =
                        CharacterObject != null
                            ? await CharacterObject.GetSkillsSectionAsync(token).ConfigureAwait(false)
                            : null;
                    bool blnSetDefaultAttribute =
                        objSkillsSection != null && (await objSkillsSection.GetKnowledgeSkillCategoriesMapAsync(token)
                            .ConfigureAwait(false))
                        .TryGetValue(value,
                            out strNewAttributeValue);
                    bool blnUnsetNativeLanguage = value != "Language" && strOldType == "Language";
                    if (blnSetDefaultAttribute || blnUnsetNativeLanguage)
                    {
                        bool blnTemp1 = false;
                        bool blnTemp2 = false;
                        token.ThrowIfCancellationRequested();
                        if (blnSetDefaultAttribute && InterlockExchangeDefaultAttribute(strNewAttributeValue) !=
                            strNewAttributeValue)
                        {
                            if (IsLoading)
                                await RecacheAttributeAsync(token).ConfigureAwait(false);
                            else
                                blnTemp1 = true;
                        }

                        if (blnUnsetNativeLanguage && Interlocked.Exchange(ref _intIsNativeLanguage, 0) == 0)
                        {
                            blnTemp2 = true;
                        }

                        if (blnTemp1)
                        {
                            if (blnTemp2)
                                await this.OnMultiplePropertyChangedAsync(token, nameof(Type), nameof(DefaultAttribute),
                                    nameof(IsNativeLanguage)).ConfigureAwait(false);
                            else
                                await this.OnMultiplePropertyChangedAsync(token, nameof(Type), nameof(DefaultAttribute))
                                    .ConfigureAwait(false);
                        }
                        else if (blnTemp2)
                            await this.OnMultiplePropertyChangedAsync(token, nameof(Type), nameof(IsNativeLanguage))
                                .ConfigureAwait(false);
                        else
                            await OnPropertyChangedAsync(nameof(Type), token).ConfigureAwait(false);
                    }
                    else
                        await OnPropertyChangedAsync(nameof(Type), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool IsLanguage => Type == "Language";

        public override async Task<bool> GetIsLanguageAsync(CancellationToken token = default)
        {
            return await GetTypeAsync(token).ConfigureAwait(false) == "Language";
        }

        public override bool IsNativeLanguage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intIsNativeLanguage > 0;
            }
            set
            {
                int intNewValue = value.ToInt32();
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_intIsNativeLanguage == intNewValue)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intIsNativeLanguage, intNewValue) == intNewValue)
                            return;
                        if (value)
                        {
                            Base = 0;
                            Karma = 0;
                            BuyWithKarma = false;
                            Specializations.Clear();
                        }

                        OnPropertyChanged();
                    }
                }
            }
        }

        public override async Task<bool> GetIsNativeLanguageAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intIsNativeLanguage > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override async Task SetIsNativeLanguageAsync(bool value, CancellationToken token = default)
        {
            int intNewValue = value.ToInt32();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Interlocked guarantees thread safety here without write lock
                if (Interlocked.Exchange(ref _intIsNativeLanguage, intNewValue) == intNewValue)
                    return;
                if (value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await SetBaseAsync(0, token).ConfigureAwait(false);
                        await SetKarmaAsync(0, token).ConfigureAwait(false);
                        await SetBuyWithKarmaAsync(false, token).ConfigureAwait(false);
                        await Specializations.ClearAsync(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                await OnPropertyChangedAsync(nameof(IsNativeLanguage), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
        /// </summary>
        /// <returns>Price in karma</returns>
        public override int UpgradeKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intTotalBaseRating = TotalBaseRating;
                    if (intTotalBaseRating >= RatingMaximum)
                    {
                        return -1;
                    }

                    int intOptionsCost;
                    int intValue;
                    if (intTotalBaseRating == 0)
                    {
                        intOptionsCost = KarmaNewSkillCost;
                        intValue = intOptionsCost;
                    }
                    else
                    {
                        intOptionsCost = KarmaImproveSkillCost;
                        intValue = (intTotalBaseRating + 1) * intOptionsCost;
                    }

                    string strDictionaryKey = DictionaryKey;
                    intValue = ImprovementManager.ApplyCostImprovements(CharacterObject,
                        KarmaCostImprovementType,
                        KarmaCostMultiplierImprovementType,
                        strDictionaryKey, intValue,
                        objImprovement => (objImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objImprovement.Maximum) &&
                                        objImprovement.Minimum <= intTotalBaseRating + 1);
                    
                    // Apply skill category karma cost improvements
                    intValue = ImprovementManager.ApplyCostImprovements(CharacterObject,
                        Improvement.ImprovementType.SkillCategoryKarmaCost,
                        Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier,
                        SkillCategory, intValue,
                        objImprovement => (objImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objImprovement.Maximum) &&
                                        objImprovement.Minimum <= intTotalBaseRating + 1);
                    
                    // Apply minimum cost override
                    intValue = SkillImprovementHelper.ApplyMinimumCostOverride(CharacterObject, KarmaCostMinimumImprovementType, strDictionaryKey, SkillCategory, intValue, intTotalBaseRating + 1);
                    
                    return Math.Max(intValue, Math.Min(1, intOptionsCost));
                }
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
        /// </summary>
        /// <returns>Price in karma</returns>
        public override async Task<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                if (intTotalBaseRating >= await GetRatingMaximumAsync(token).ConfigureAwait(false))
                {
                    return -1;
                }

                int intOptionsCost;
                int intValue;
                if (intTotalBaseRating == 0)
                {
                    intOptionsCost = await GetKarmaNewSkillCostAsync(token).ConfigureAwait(false);
                    intValue = intOptionsCost;
                }
                else
                {
                    intOptionsCost = await GetKarmaImproveSkillCostAsync(token).ConfigureAwait(false);
                    intValue = (intTotalBaseRating + 1) * intOptionsCost;
                }

                string strDictionaryKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                intValue = await ImprovementManager.ApplyCostImprovementsAsync(CharacterObject,
                    KarmaCostImprovementType,
                    KarmaCostMultiplierImprovementType,
                    strDictionaryKey, intValue,
                    objImprovement => (objImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objImprovement.Maximum) &&
                                    objImprovement.Minimum <= intTotalBaseRating + 1, token).ConfigureAwait(false);
                
                // Apply skill category karma cost improvements
                intValue = await ImprovementManager.ApplyCostImprovementsAsync(CharacterObject,
                    Improvement.ImprovementType.SkillCategoryKarmaCost,
                    Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier,
                    SkillCategory, intValue,
                    objImprovement => (objImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objImprovement.Maximum) &&
                                    objImprovement.Minimum <= intTotalBaseRating + 1, token).ConfigureAwait(false);
                
                // Apply minimum cost override
                intValue = await SkillImprovementHelper.ApplyMinimumCostOverrideAsync(CharacterObject, KarmaCostMinimumImprovementType, strDictionaryKey, SkillCategory, intValue, intTotalBaseRating + 1, token).ConfigureAwait(false);
                
                return Math.Max(intValue, Math.Min(1, intOptionsCost));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        public override int CurrentSpCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intBasePoints = BasePoints;
                    int intPointCost = intBasePoints;
                    if (!IsExoticSkill && !BuyWithKarma)
                        intPointCost += Specializations.Count(x => !x.Free);

                    string strDictionaryKey = DictionaryKey;
                    intPointCost = ImprovementManager.ApplyCostImprovements(CharacterObject, 
                        Improvement.ImprovementType.KnowledgeSkillPointCost, 
                        Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier, 
                        strDictionaryKey, intPointCost,
                        objImprovement => objImprovement.Minimum <= intBasePoints);
                    intPointCost = ImprovementManager.ApplyCostImprovements(CharacterObject, 
                        Improvement.ImprovementType.SkillCategoryPointCost, 
                        Improvement.ImprovementType.SkillCategoryPointCostMultiplier, 
                        SkillCategory, intPointCost,
                        objImprovement => objImprovement.Minimum <= intBasePoints);

                    return Math.Max(intPointCost, 0);
                }
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        public override async Task<int> GetCurrentSpCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBasePoints = await GetBasePointsAsync(token).ConfigureAwait(false);
                int cost = intBasePoints;
                if (!IsExoticSkill && !await GetBuyWithKarmaAsync(token).ConfigureAwait(false))
                    cost += await Specializations.CountAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);

                string strDictionaryKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                cost = await ImprovementManager.ApplyCostImprovementsAsync(CharacterObject, 
                    Improvement.ImprovementType.KnowledgeSkillPointCost, 
                    Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier, 
                    strDictionaryKey, cost,
                    objImprovement => objImprovement.Minimum <= intBasePoints, token).ConfigureAwait(false);
                cost = await ImprovementManager.ApplyCostImprovementsAsync(CharacterObject, 
                    Improvement.ImprovementType.SkillCategoryPointCost, 
                    Improvement.ImprovementType.SkillCategoryPointCostMultiplier, 
                    SkillCategory, cost,
                    objImprovement => objImprovement.Minimum <= intBasePoints, token).ConfigureAwait(false);

                return Math.Max(cost, 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override void WriteToDerived(XmlWriter objWriter)
        {
            objWriter.WriteElementString("type", Type);
            objWriter.WriteElementString("isnativelanguage", IsNativeLanguage.ToString(GlobalSettings.InvariantCultureInfo));
            if (ForcedName)
                objWriter.WriteElementString("forced", null);
        }

        public void Load(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            using (LockObject.EnterWriteLock())
            {
                string strTemp = Name;
                if (xmlNode.TryGetStringFieldQuickly("name", ref strTemp))
                    Name = strTemp;
                if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                    SkillId = guiTemp;
                else if (xmlNode.TryGetField("suid", Guid.TryParse, out Guid guiTemp2))
                    SkillId = guiTemp2;

                bool blnTemp = false;
                if (xmlNode.TryGetBoolFieldQuickly("disableupgrades", ref blnTemp))
                    _blnAllowUpgrade = !blnTemp;

                // Legacy shim
                if (SkillId.Equals(Guid.Empty))
                {
                    XPathNavigator objDataNode = CharacterObject.LoadDataXPath("skills.xml")
                        .TryGetNodeByNameOrId("/chummer/knowledgeskills/skill", Name);
                    if (objDataNode.TryGetField("id", Guid.TryParse, out Guid guidTemp))
                        SkillId = guidTemp;
                }

                string strCategoryString = string.Empty;
                if ((xmlNode.TryGetStringFieldQuickly("type", ref strCategoryString) &&
                     !string.IsNullOrEmpty(strCategoryString))
                    || (xmlNode.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) &&
                        !string.IsNullOrEmpty(strCategoryString)))
                {
                    Type = strCategoryString;
                }

                // Legacy sweep for native language skills
                blnTemp = false;
                if (!xmlNode.TryGetBoolFieldQuickly("isnativelanguage", ref blnTemp) && IsLanguage &&
                    CharacterObject.LastSavedVersion <= new ValueVersion(5, 212, 72))
                {
                    int intKarma = 0;
                    int intBase = 0;
                    xmlNode.TryGetInt32FieldQuickly("karma", ref intKarma);
                    xmlNode.TryGetInt32FieldQuickly("base", ref intBase);
                    if (intKarma == 0 && intBase == 0 &&
                        CharacterObject.SkillsSection.KnowledgeSkills.Count(x => x.IsNativeLanguage) < 1 +
                        ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.NativeLanguageLimit))
                        blnTemp = true;
                }

                _intIsNativeLanguage = blnTemp.ToInt32();
            }
        }

        public async Task LoadAsync(XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strTemp = await GetNameAsync(token).ConfigureAwait(false);
                if (xmlNode.TryGetStringFieldQuickly("name", ref strTemp))
                    await SetNameAsync(strTemp, token).ConfigureAwait(false);
                if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                    await SetSkillIdAsync(guiTemp, token).ConfigureAwait(false);
                else if (xmlNode.TryGetField("suid", Guid.TryParse, out Guid guiTemp2))
                    await SetSkillIdAsync(guiTemp2, token).ConfigureAwait(false);

                bool blnTemp = false;
                if (xmlNode.TryGetBoolFieldQuickly("disableupgrades", ref blnTemp))
                    _blnAllowUpgrade = !blnTemp;

                // Legacy shim
                if (SkillId.Equals(Guid.Empty))
                {
                    XPathNavigator objDataNode = (await CharacterObject.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                        .TryGetNodeByNameOrId("/chummer/knowledgeskills/skill", await GetNameAsync(token).ConfigureAwait(false));
                    if (objDataNode.TryGetField("id", Guid.TryParse, out Guid guidTemp))
                        await SetSkillIdAsync(guidTemp, token).ConfigureAwait(false);
                }

                string strCategoryString = string.Empty;
                if ((xmlNode.TryGetStringFieldQuickly("type", ref strCategoryString) &&
                     !string.IsNullOrEmpty(strCategoryString))
                    || (xmlNode.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) &&
                        !string.IsNullOrEmpty(strCategoryString)))
                {
                    await SetTypeAsync(strCategoryString, token).ConfigureAwait(false);
                }

                // Legacy sweep for native language skills
                blnTemp = false;
                if (!xmlNode.TryGetBoolFieldQuickly("isnativelanguage", ref blnTemp) && await GetIsLanguageAsync(token).ConfigureAwait(false) &&
                    CharacterObject.LastSavedVersion <= new ValueVersion(5, 212, 72))
                {
                    int intKarma = 0;
                    int intBase = 0;
                    xmlNode.TryGetInt32FieldQuickly("karma", ref intKarma);
                    xmlNode.TryGetInt32FieldQuickly("base", ref intBase);
                    if (intKarma == 0 && intBase == 0 &&
                        await (await (await CharacterObject.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowledgeSkillsAsync(token).ConfigureAwait(false))
                            .CountAsync(x => x.GetIsNativeLanguageAsync(token), token).ConfigureAwait(false) < 1 +
                        await ImprovementManager.ValueOfAsync(CharacterObject, Improvement.ImprovementType.NativeLanguageLimit, token: token).ConfigureAwait(false))
                        blnTemp = true;
                }

                _intIsNativeLanguage = blnTemp.ToInt32();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
