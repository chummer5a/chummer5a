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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Chummer.Backend.Enums;
using Microsoft.VisualStudio.Threading;

namespace Chummer
{
    public static class SettingsManager
    {
        private static int _intDicLoadedCharacterSettingsLoadedStatus = -1;
        private static readonly ConcurrentDictionary<string, CharacterSettings> s_DicLoadedCharacterSettings = new ConcurrentDictionary<string, CharacterSettings>();
        private static readonly FileSystemWatcher s_ObjSettingsFolderWatcher;
        private static readonly DebuggableSemaphoreSlim s_ObjSettingsFolderWatcherSemaphore;

        static SettingsManager()
        {
            string strSettingsPath = Utils.GetSettingsFolderPath;
            if (!Directory.Exists(strSettingsPath))
            {
                try
                {
                    Directory.CreateDirectory(strSettingsPath);
                }
                catch (Exception)
                {
                    return;
                }
            }

            s_ObjSettingsFolderWatcherSemaphore = new DebuggableSemaphoreSlim();
            s_ObjSettingsFolderWatcher = new FileSystemWatcher(strSettingsPath, "*.xml");
            s_ObjSettingsFolderWatcher.BeginInit();
            s_ObjSettingsFolderWatcher.Created += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Deleted += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Changed += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Renamed += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.EnableRaisingEvents = true;
            s_ObjSettingsFolderWatcher.EndInit();
        }

        private static async void ObjSettingsFolderWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync().ConfigureAwait(false);
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        await s_ObjSettingsFolderWatcherSemaphore.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            await AddSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        }
                        finally
                        {
                            s_ObjSettingsFolderWatcherSemaphore.Release();
                        }
                        break;

                    case WatcherChangeTypes.Deleted:
                        await s_ObjSettingsFolderWatcherSemaphore.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            await RemoveSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        }
                        finally
                        {
                            s_ObjSettingsFolderWatcherSemaphore.Release();
                        }
                        break;

                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Renamed:
                        await s_ObjSettingsFolderWatcherSemaphore.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            await ReloadSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        }
                        finally
                        {
                            s_ObjSettingsFolderWatcherSemaphore.Release();
                        }
                        break;
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static IReadOnlyDictionary<string, CharacterSettings> LoadedCharacterSettings => LoadedCharacterSettingsAsModifiable;

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static async Task<IReadOnlyDictionary<string, CharacterSettings>> GetLoadedCharacterSettingsAsync(CancellationToken token = default)
        {
            return await GetLoadedCharacterSettingsAsModifiableAsync(token).ConfigureAwait(false);
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static ConcurrentDictionary<string, CharacterSettings> LoadedCharacterSettingsAsModifiable
        {
            get
            {
                do
                {
                    try
                    {
                        if (Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 0, -1)
                            < 0) // Makes sure if we end up calling this from multiple threads, only one does loading at a time
                            LoadCharacterSettings();
                    }
                    catch
                    {
                        _intDicLoadedCharacterSettingsLoadedStatus = -1;
                        throw;
                    }

                    while (_intDicLoadedCharacterSettingsLoadedStatus <= 1
                           && _intDicLoadedCharacterSettingsLoadedStatus >= 0)
                        Utils.SafeSleep();
                }
                while (_intDicLoadedCharacterSettingsLoadedStatus < 0);
                return s_DicLoadedCharacterSettings;
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static async Task<ConcurrentDictionary<string, CharacterSettings>> GetLoadedCharacterSettingsAsModifiableAsync(CancellationToken token = default)
        {
            do
            {
                try
                {
                    if (Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 0, -1)
                        < 0) // Makes sure if we end up calling this from multiple threads, only one does loading at a time
                        await LoadCharacterSettingsAsync(token).ConfigureAwait(false);
                }
                catch
                {
                    _intDicLoadedCharacterSettingsLoadedStatus = -1;
                    throw;
                }

                while (_intDicLoadedCharacterSettingsLoadedStatus <= 1
                       && _intDicLoadedCharacterSettingsLoadedStatus >= 0)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            } while (_intDicLoadedCharacterSettingsLoadedStatus < 0);

            return s_DicLoadedCharacterSettings;
        }

        private static void LoadCharacterSettings(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _intDicLoadedCharacterSettingsLoadedStatus = 0;
            List<CharacterSettings> lstSettings = s_DicLoadedCharacterSettings.GetValuesToListSafe();
            s_DicLoadedCharacterSettings.Clear();
            foreach (CharacterSettings objSettings in lstSettings)
                objSettings.Dispose();
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
            {
                CharacterSettings objNewCharacterSettings = new CharacterSettings();
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!s_DicLoadedCharacterSettings.TryAdd(GlobalSettings.DefaultCharacterSetting,
                                                             objNewCharacterSettings))
                        objNewCharacterSettings.Dispose();
                }
                catch
                {
                    objNewCharacterSettings.Dispose();
                    throw;
                }
                finally
                {
                    Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 2, 0);
                }

                return;
            }

            Utils.RunWithoutThreadLock(() =>
            {
                IEnumerable<XPathNavigator> xmlSettingsIterator
                    = XmlManager.LoadXPath("settings.xml", token: token).SelectAndCacheExpression("/chummer/settings/setting", token)
                    .Cast<XPathNavigator>();
                Parallel.ForEach(xmlSettingsIterator, xmlBuiltInSetting =>
                {
                    CharacterSettings objNewCharacterSettings = new CharacterSettings();
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!objNewCharacterSettings.Load(xmlBuiltInSetting, token)
                            || (objNewCharacterSettings.BuildMethodIsLifeModule
                                && !GlobalSettings.LifeModuleEnabled)
                            || !s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                    objNewCharacterSettings))
                            objNewCharacterSettings.Dispose();
                    }
                    catch
                    {
                        objNewCharacterSettings.Dispose();
                        throw;
                    }
                });
            }, token);
            if (Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 1, 0) != 0)
                return;

            string strSettingsPath = Utils.GetSettingsFolderPath;
            if (Directory.Exists(strSettingsPath))
            {
                Utils.RunWithoutThreadLock(() =>
                {
                    Parallel.ForEach(Directory.EnumerateFiles(strSettingsPath, "*.xml"), strSettingsFilePath =>
                    {
                        string strSettingName = Path.GetFileName(strSettingsFilePath);
                        CharacterSettings objNewCharacterSettings = new CharacterSettings();
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (!objNewCharacterSettings.Load(strSettingName, false, false, token)
                                || (objNewCharacterSettings.BuildMethodIsLifeModule
                                    && !GlobalSettings.LifeModuleEnabled)
                                || !s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                        objNewCharacterSettings))
                                objNewCharacterSettings.Dispose();
                        }
                        catch
                        {
                            objNewCharacterSettings.Dispose();
                            throw;
                        }
                    });
                }, token);
            }

            Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 2, 1);
        }

        private static async Task LoadCharacterSettingsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _intDicLoadedCharacterSettingsLoadedStatus = 0;
            List<CharacterSettings> lstSettings = s_DicLoadedCharacterSettings.GetValuesToListSafe();
            s_DicLoadedCharacterSettings.Clear();
            foreach (CharacterSettings objSettings in lstSettings)
                await objSettings.DisposeAsync().ConfigureAwait(false);
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
            {
                CharacterSettings objNewCharacterSettings = new CharacterSettings();
                try
                {
                    if (!s_DicLoadedCharacterSettings.TryAdd(GlobalSettings.DefaultCharacterSetting,
                            objNewCharacterSettings))
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 2, 0);
                }

                return;
            }

            await ParallelExtensions.ForEachAsync((await XmlManager.LoadXPathAsync("settings.xml", token: token)
                    .ConfigureAwait(false)).SelectAndCacheExpression("/chummer/settings/setting", token: token),
                xmlBuiltInSetting => LoadSettings(xmlBuiltInSetting as XPathNavigator), token).ConfigureAwait(false);

            async Task LoadSettings(XPathNavigator xmlBuiltInSetting)
            {
                CharacterSettings objNewCharacterSettings = new CharacterSettings();
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!await objNewCharacterSettings.LoadAsync(xmlBuiltInSetting, token)
                                                      .ConfigureAwait(false)
                        || (!GlobalSettings.LifeModuleEnabled
                            && await objNewCharacterSettings.GetBuildMethodIsLifeModuleAsync(token)
                                                            .ConfigureAwait(false))
                        || !s_DicLoadedCharacterSettings.TryAdd(
                            await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false),
                            objNewCharacterSettings))
                    {
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
            }
            if (Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 1, 0) != 0)
                return;

            string strSettingsPath = Utils.GetSettingsFolderPath;
            if (Directory.Exists(strSettingsPath))
            {
                await ParallelExtensions.ForEachAsync(Directory.EnumerateFiles(strSettingsPath, "*.xml"), LoadSettingsFromFile, token).ConfigureAwait(false);
                
                async Task LoadSettingsFromFile(string strSettingsFilePath)
                {
                    token.ThrowIfCancellationRequested();
                    string strSettingName = Path.GetFileName(strSettingsFilePath);
                    CharacterSettings objNewCharacterSettings = new CharacterSettings();
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!await objNewCharacterSettings.LoadAsync(strSettingName, false, false, token)
                                                          .ConfigureAwait(false)
                            || (!GlobalSettings.LifeModuleEnabled
                                && await objNewCharacterSettings.GetBuildMethodIsLifeModuleAsync(token).ConfigureAwait(false))
                            || !s_DicLoadedCharacterSettings.TryAdd(
                                await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                objNewCharacterSettings))
                        {
                            await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                }
            }
            Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 2, 1);
        }

        private static async Task AddSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            try
            {
                if (!await objNewCharacterSettings.LoadAsync(strSettingName, false, true, token).ConfigureAwait(false)
                    || (objNewCharacterSettings.BuildMethodIsLifeModule
                        && !GlobalSettings.LifeModuleEnabled))
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                string strKey = await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                CharacterSettings objSettingsLoaded = await s_DicLoadedCharacterSettings.AddOrUpdateAsync(
                    strKey, objNewCharacterSettings,
                    async (x, objOldCharacterSettings) =>
                    {
                        await objOldCharacterSettings
                              .CopyValuesAsync(
                                  objNewCharacterSettings,
                                  token: token)
                              .ConfigureAwait(false);
                        return objOldCharacterSettings;
                    }, token).ConfigureAwait(false);

                if (objSettingsLoaded != objNewCharacterSettings)
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                throw;
            }

            await GlobalSettings.ReloadCustomSourcebookInfosAsync(token).ConfigureAwait(false);
        }

        private static async Task RemoveSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objSettingsToDelete
                = s_DicLoadedCharacterSettings.FirstOrDefault(x => x.Value.FileName == strSettingName).Value;
            if (objSettingsToDelete == default)
                return;
            string strKeyToDelete = await objSettingsToDelete.GetDictionaryKeyAsync(token).ConfigureAwait(false);

            try
            {
                AsyncLazy<string> strBestMatchForCreatedNewSettingsKey = new AsyncLazy<string>(async () =>
                {
                    int intBestScore = int.MinValue;
                    string strReturn = string.Empty;
                    await s_DicLoadedCharacterSettings.ForEachAsync(async x =>
                    {
                        if (strKeyToDelete == x.Key)
                            return;
                        // ReSharper disable once AccessToDisposedClosure
                        int intLoopScore
                            = await CalculateCharacterSettingsMatchScore(
                                objSettingsToDelete, x.Value, true, token).ConfigureAwait(false);
                        if (intLoopScore > intBestScore)
                        {
                            intBestScore = intLoopScore;
                            strReturn = x.Key;
                        }
                    }, token: token).ConfigureAwait(false);
                    return strReturn;
                }, Utils.JoinableTaskFactory);
                AsyncLazy<string> strBestMatchForNonCreatedNewSettingsKey = new AsyncLazy<string>(async () =>
                {
                    int intBestScore = int.MinValue;
                    string strReturn = string.Empty;
                    await s_DicLoadedCharacterSettings.ForEachAsync(async x =>
                    {
                        if (strKeyToDelete == x.Key)
                            return;
                        // ReSharper disable once AccessToDisposedClosure
                        int intLoopScore
                            = await CalculateCharacterSettingsMatchScore(
                                objSettingsToDelete, x.Value, false, token).ConfigureAwait(false);
                        if (intLoopScore > intBestScore)
                        {
                            intBestScore = intLoopScore;
                            strReturn = x.Key;
                        }
                    }, token: token).ConfigureAwait(false);
                    return strReturn;
                }, Utils.JoinableTaskFactory);
                await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                {
                    System.IAsyncDisposable objLocker2 = await objCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (await objCharacter.GetSettingsKeyAsync(token).ConfigureAwait(false) == strKeyToDelete)
                        {
                            await objCharacter
                                .SetSettingsKeyAsync(await objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                                ? await strBestMatchForCreatedNewSettingsKey.GetValueAsync(token).ConfigureAwait(false)
                                : await strBestMatchForNonCreatedNewSettingsKey.GetValueAsync(token).ConfigureAwait(false),
                                    token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                if (s_DicLoadedCharacterSettings.TryRemove(strKeyToDelete, out CharacterSettings objRemovedSetting)
                    && !ReferenceEquals(objRemovedSetting, objSettingsToDelete))
                    await objRemovedSetting.DisposeAsync().ConfigureAwait(false);
                await objSettingsToDelete.DisposeAsync().ConfigureAwait(false);
            }

            await GlobalSettings.ReloadCustomSourcebookInfosAsync(token).ConfigureAwait(false);
        }

        private static async Task ReloadSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            try
            {
                if (!await objNewCharacterSettings.LoadAsync(strSettingName, false, true, token).ConfigureAwait(false)
                    || (objNewCharacterSettings.BuildMethodIsLifeModule
                        && !GlobalSettings.LifeModuleEnabled))
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                string strKey = await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                CharacterSettings objSettingsLoaded = await s_DicLoadedCharacterSettings.AddOrUpdateAsync(
                    strKey, objNewCharacterSettings,
                    async (x, objOldCharacterSettings) =>
                    {
                        await objOldCharacterSettings
                              .CopyValuesAsync(
                                  objNewCharacterSettings,
                                  token: token)
                              .ConfigureAwait(false);
                        return objOldCharacterSettings;
                    }, token).ConfigureAwait(false);

                // We ended up changing our dictionary key, so find the first custom setting without a corresponding file and delete it
                // (we assume that it's the one that got renamed)
                if (objSettingsLoaded == objNewCharacterSettings)
                {
                    CharacterSettings objToDelete = (await s_DicLoadedCharacterSettings.FirstOrDefaultAsync(
                            async x => !await x.Value.GetBuiltInOptionAsync(token)
                                               .ConfigureAwait(false)
                                       && !File.Exists(
                                           Path.Combine(Utils.GetSettingsFolderPath,
                                                        await x.Value.GetFileNameAsync(token)
                                                               .ConfigureAwait(false))), token)
                        .ConfigureAwait(false)).Value;
                    if (objToDelete != null)
                    {
                        string strKeyToDelete = await objToDelete.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                        await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                        {
                            if (await objCharacter.GetSettingsKeyAsync(token).ConfigureAwait(false) == strKeyToDelete)
                                await objCharacter.SetSettingsKeyAsync(strKey, token).ConfigureAwait(false);
                        }, token: token).ConfigureAwait(false);
                        if (s_DicLoadedCharacterSettings.TryRemove(strKeyToDelete, out CharacterSettings objRemovedSetting)
                            && !ReferenceEquals(objRemovedSetting, objToDelete))
                            await objRemovedSetting.DisposeAsync().ConfigureAwait(false);
                        await objToDelete.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                throw;
            }

            await GlobalSettings.ReloadCustomSourcebookInfosAsync(token).ConfigureAwait(false);
        }

        private static async Task<int> CalculateCharacterSettingsMatchScore(CharacterSettings objBaselineSettings, CharacterSettings objOptionsToCheck, bool blnForCreatedCharacter, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            int intBaseline = await objOptionsToCheck.GetBuiltInOptionAsync(token).ConfigureAwait(false) ? 5 : 4;
            CharacterBuildMethod eLeftBuildMethod = await objBaselineSettings.GetBuildMethodAsync(token).ConfigureAwait(false);
            CharacterBuildMethod eRightBuildMethod = await objOptionsToCheck.GetBuildMethodAsync(token).ConfigureAwait(false);
            int intDeltaMaxKarma = await objBaselineSettings.GetBuildKarmaAsync(token).ConfigureAwait(false) -  await objOptionsToCheck.GetBuildKarmaAsync(token).ConfigureAwait(false);
            decimal decDeltaMaxNuyen = await objBaselineSettings.GetNuyenMaximumBPAsync(token).ConfigureAwait(false) - await objOptionsToCheck.GetNuyenMaximumBPAsync(token).ConfigureAwait(false);
            if (blnForCreatedCharacter)
            {
                if (eLeftBuildMethod != eRightBuildMethod)
                {
                    if (eLeftBuildMethod.UsesPriorityTables() == eRightBuildMethod.UsesPriorityTables())
                        intReturn -= 2;
                    else
                        intReturn -= 4;
                }
                if (intDeltaMaxKarma != 0)
                    intReturn -= Math.Min(Math.Abs(intDeltaMaxKarma), 2);
                if (decDeltaMaxNuyen != 0)
                    intReturn -= Math.Min(Math.Abs(decDeltaMaxNuyen).StandardRound(), 2);

            }
            else
            {
                if (eLeftBuildMethod != eRightBuildMethod)
                {
                    if (eLeftBuildMethod.UsesPriorityTables() == eRightBuildMethod.UsesPriorityTables())
                    {
                        intBaseline += 2;
                        intReturn -= int.MaxValue / 2;
                    }
                    else
                    {
                        intBaseline += 4;
                        intReturn -= int.MaxValue;
                    }
                }
                intReturn -= (intDeltaMaxKarma.Pow(2) + decDeltaMaxNuyen.Pow(2).StandardRound()).FastSqrtAndStandardRound();
            }

            IReadOnlyList<CustomDataDirectoryInfo> setBaselineCustomDataDirectoryInfos = await objBaselineSettings.GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false);
            IReadOnlyList<CustomDataDirectoryInfo> setCheckCustomDataDirectoryInfos = await objOptionsToCheck.GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false);
            int intBaselineCustomDataCount = setCheckCustomDataDirectoryInfos.Count;
            if (intBaselineCustomDataCount == 0)
            {
                intBaselineCustomDataCount = setBaselineCustomDataDirectoryInfos.Count;
                if (intBaselineCustomDataCount > 0)
                {
                    intReturn -= intBaselineCustomDataCount.Pow(2) * intBaseline;
                }
            }
            else if (setBaselineCustomDataDirectoryInfos.Count == 0)
            {
                intReturn -= intBaselineCustomDataCount.Pow(2) * intBaseline;
            }
            else
            {
                intBaselineCustomDataCount = Math.Max(setBaselineCustomDataDirectoryInfos.Count, intBaselineCustomDataCount);
                for (int i = 0;
                     i < setCheckCustomDataDirectoryInfos.Count;
                     ++i)
                {
                    string strLoopCustomDataName = setCheckCustomDataDirectoryInfos[i].Name;
                    int intLoopIndex = setBaselineCustomDataDirectoryInfos.FindIndex(x => x.Name == strLoopCustomDataName);
                    if (intLoopIndex < 0)
                        intReturn -= intBaselineCustomDataCount * intBaseline;
                    else
                        intReturn -= Math.Abs(i - intLoopIndex) * intBaseline;
                }

                int intMismatchCount = setBaselineCustomDataDirectoryInfos.Count(x =>
                {
                    string strInner = x.Name;
                    return setCheckCustomDataDirectoryInfos.All(y => y.Name != strInner);
                });
                if (intMismatchCount != 0)
                    intReturn -= intMismatchCount * intBaselineCustomDataCount * intBaseline;
            }

            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(
                       Utils.StringHashSetPool, out HashSet<string> setDummyBooks))
            {
                setDummyBooks.AddRange(await objBaselineSettings.GetBooksAsync(token).ConfigureAwait(false));
                IReadOnlyCollection<string> setNewBooks = await objOptionsToCheck.GetBooksAsync(token).ConfigureAwait(false);
                int intExtraBooks = setNewBooks.Count(x => !setDummyBooks.Remove(x));
                setDummyBooks.ExceptWith(setNewBooks);
                // Missing books are weighted a lot more heavily than extra books
                intReturn -= (setDummyBooks.Count * (intBaselineCustomDataCount + byte.MaxValue)
                              + intExtraBooks) * intBaseline;
            }

            return intReturn;
        }
    }
}
