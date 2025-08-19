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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Skills
{
    public sealed class ExoticSkill : Skill
    {
        private string _strSpecific;

        public ExoticSkill(Character character, XmlNode node, bool blnDoSkillGroup = true) : base(character, node, blnDoSkillGroup)
        {
        }

        public void Load(XmlNode node)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, node));
        }

        public Task LoadAsync(XmlNode node, CancellationToken token = default)
        {
            return LoadCoreAsync(false, node, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode node, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (node == null)
                return;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (node.TryGetStringFieldQuickly("specific", ref _strSpecific)
                    && _strSpecific.StartsWith("Elektro-") && CharacterObject.LastSavedVersion < new ValueVersion(5, 255, 949))
                {
                    // Legacy shim
                    switch (_strSpecific)
                    {
                        case "Elektro-Netz":
                            _strSpecific = "Electro-Net";
                            break;
                        case "Elektro-Angel":
                            _strSpecific = "Electro-Fishing Rod";
                            break;
                    }
                }
            }
            finally
            {
                if (blnSync)
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static bool IsExoticSkillName(Character objCharacter, string strSkillName,
                                             CancellationToken token = default)
        {
            return IsExoticSkillNameTuple(objCharacter, strSkillName, token).Item1;
        }

        public static Tuple<bool, string> IsExoticSkillNameTuple(Character objCharacter, string strSkillName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strSkillName))
                return new Tuple<bool, string>(false, string.Empty);
            XPathNodeIterator objXPathNameData = objCharacter.LoadDataXPath("skills.xml", token: token)
                                                             .SelectAndCacheExpression(
                                                                 "/chummer/skills/skill[exotic = 'True']/name", token);
            foreach (XPathNavigator objData in objXPathNameData)
            {
                token.ThrowIfCancellationRequested();
                if (strSkillName.StartsWith(objData.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return new Tuple<bool, string>(true, objData.Value);
                }
            }
            return new Tuple<bool, string>(false, string.Empty);
        }

        public static async Task<bool> IsExoticSkillNameAsync(Character objCharacter, string strSkillName,
                                                                   CancellationToken token = default)
        {
            return (await IsExoticSkillNameTupleAsync(objCharacter, strSkillName, token).ConfigureAwait(false)).Item1;
        }

        public static async Task<Tuple<bool, string>> IsExoticSkillNameTupleAsync(Character objCharacter, string strSkillName,
                                                                   CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strSkillName))
                return new Tuple<bool, string>(false, string.Empty);
            XPathNodeIterator objXPathNameData
                = (await objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                        .SelectAndCacheExpression("/chummer/skills/skill[exotic = 'True']/name", token);
            foreach (XPathNavigator objData in objXPathNameData)
            {
                token.ThrowIfCancellationRequested();
                if (strSkillName.StartsWith(objData.Value, StringComparison.OrdinalIgnoreCase))
                    return new Tuple<bool, string>(true, objData.Value);
            }

            return new Tuple<bool, string>(false, string.Empty);
        }

        public override bool IsExoticSkill => true;

        public override string DictionaryKey
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _strDictionaryKey = _strDictionaryKey ?? Name + " (" + Specific + ')';
                }
            }
        }

        public override async Task<string> GetDictionaryKeyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strDictionaryKey = _strDictionaryKey
                                           ?? await GetNameAsync(token).ConfigureAwait(false) + " (" +
                                           await GetSpecificAsync(token).ConfigureAwait(false) + ')';
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool AllowDelete
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !CharacterObject.Created && FreeBase + FreeKarma + RatingModifiers(Attribute) <= 0;
            }
        }

        public override async Task<bool> GetAllowDeleteAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                       && await GetFreeBaseAsync(token).ConfigureAwait(false)
                       + await GetFreeKarmaAsync(token).ConfigureAwait(false)
                       + await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false) <= 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override bool BuyWithKarma
        {
            get => false;
            set
            {
                // Dummy
            }
        }

        public override void WriteToDerived(XmlWriter objWriter)
        {
            objWriter.WriteElementString("specific", Specific);
        }

        public string Specific
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSpecific;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _strSpecific, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetSpecificAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strSpecific;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetSpecificAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _strSpecific, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(Specific), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplaySpecific(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                    ? Specific
                    : CharacterObject.TranslateExtra(Specific, strLanguage);
            }
        }

        public async Task<string> DisplaySpecificAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                    ? Specific
                    : await CharacterObject.TranslateExtraAsync(Specific, strLanguage, token: token)
                        .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplaySpecific => DisplaySpecific(GlobalSettings.Language);

        public Task<string> GetCurrentDisplaySpecificAsync(CancellationToken token = default) => DisplaySpecificAsync(GlobalSettings.Language, token);

        public override string DisplaySpecialization(string strLanguage)
        {
            return DisplaySpecific(strLanguage);
        }

        public override Task<string> DisplaySpecializationAsync(string strLanguage, CancellationToken token = default)
        {
            return DisplaySpecificAsync(strLanguage, token);
        }
    }
}
