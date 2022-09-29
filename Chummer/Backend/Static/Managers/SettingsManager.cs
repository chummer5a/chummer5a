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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public static class SettingsManager
    {
        private static int _intDicLoadedCharacterSettingsLoadedStatus = -1;
        private static readonly LockingDictionary<string, CharacterSettings> s_DicLoadedCharacterSettings = new LockingDictionary<string, CharacterSettings>();
        private static readonly FileSystemWatcher s_ObjSettingsFolderWatcher;

        static SettingsManager()
        {
            string strSettingsPath = Path.Combine(Utils.GetStartupPath, "settings");
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

            s_ObjSettingsFolderWatcher = new FileSystemWatcher(strSettingsPath, "*.xml");
            s_ObjSettingsFolderWatcher.Created += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Deleted += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Changed += ObjSettingsFolderWatcherOnChanged;
            s_ObjSettingsFolderWatcher.Renamed += ObjSettingsFolderWatcherOnChanged;
        }

        private static async void ObjSettingsFolderWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            using (await CursorWait.NewAsync().ConfigureAwait(false))
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        await AddSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        break;

                    case WatcherChangeTypes.Deleted:
                        await RemoveSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        break;

                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Renamed:
                        await ReloadSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath)).ConfigureAwait(false);
                        break;
                }
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static IAsyncReadOnlyDictionary<string, CharacterSettings> LoadedCharacterSettings
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

                    while (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                        Utils.SafeSleep();
                }
                while (_intDicLoadedCharacterSettingsLoadedStatus < 0);
                return s_DicLoadedCharacterSettings;
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static async ValueTask<IAsyncReadOnlyDictionary<string, CharacterSettings>> GetLoadedCharacterSettingsAsync(CancellationToken token = default)
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
                       && _intDicLoadedCharacterSettingsLoadedStatus > 0)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            } while (_intDicLoadedCharacterSettingsLoadedStatus < 0);

            return s_DicLoadedCharacterSettings;
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static LockingDictionary<string, CharacterSettings> LoadedCharacterSettingsAsModifiable
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

                    while (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                        Utils.SafeSleep();
                }
                while (_intDicLoadedCharacterSettingsLoadedStatus < 0);
                return s_DicLoadedCharacterSettings;
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static async ValueTask<LockingDictionary<string, CharacterSettings>> GetLoadedCharacterSettingsAsModifiableAsync(CancellationToken token = default)
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
                       && _intDicLoadedCharacterSettingsLoadedStatus > 0)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            } while (_intDicLoadedCharacterSettingsLoadedStatus < 0);

            return s_DicLoadedCharacterSettings;
        }

        private static void LoadCharacterSettings()
        {
            _intDicLoadedCharacterSettingsLoadedStatus = 0;
            try
            {
                s_DicLoadedCharacterSettings.Clear();
                try
                {
                    if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                    {
                        CharacterSettings objNewCharacterSettings = new CharacterSettings();
                        if (!s_DicLoadedCharacterSettings.TryAdd(GlobalSettings.DefaultCharacterSetting,
                                                                 objNewCharacterSettings))
                            objNewCharacterSettings.Dispose();
                        return;
                    }

                    Utils.RunWithoutThreadLock(async () =>
                    {
                        IEnumerable<XPathNavigator> xmlSettingsIterator
                            = (await (await XmlManager.LoadXPathAsync("settings.xml").ConfigureAwait(false))
                                     .SelectAndCacheExpressionAsync("/chummer/settings/setting").ConfigureAwait(false)).Cast<XPathNavigator>();
                        Parallel.ForEach(xmlSettingsIterator, xmlBuiltInSetting =>
                        {
                            CharacterSettings objNewCharacterSettings = new CharacterSettings();
                            if (!objNewCharacterSettings.Load(xmlBuiltInSetting)
                                || (objNewCharacterSettings.BuildMethodIsLifeModule
                                    && !GlobalSettings.LifeModuleEnabled)
                                || !s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                        objNewCharacterSettings))
                                objNewCharacterSettings.Dispose();
                        });
                    });
                }
                finally
                {
                    Interlocked.Increment(ref _intDicLoadedCharacterSettingsLoadedStatus);
                }

                Utils.RunWithoutThreadLock(() =>
                {
                    string strSettingsPath = Path.Combine(Utils.GetStartupPath, "settings");
                    if (Directory.Exists(strSettingsPath))
                    {
                        Parallel.ForEach(Directory.EnumerateFiles(strSettingsPath, "*.xml"), strSettingsFilePath =>
                        {
                            string strSettingName = Path.GetFileName(strSettingsFilePath);
                            CharacterSettings objNewCharacterSettings = new CharacterSettings();
                            if (!objNewCharacterSettings.Load(strSettingName, false)
                                || (objNewCharacterSettings.BuildMethodIsLifeModule
                                    && !GlobalSettings.LifeModuleEnabled)
                                || !s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                        objNewCharacterSettings))
                                objNewCharacterSettings.Dispose();
                        });
                    }
                });
            }
            finally
            {
                Interlocked.Increment(ref _intDicLoadedCharacterSettingsLoadedStatus);
            }
        }

        private static async ValueTask LoadCharacterSettingsAsync(CancellationToken token = default)
        {
            _intDicLoadedCharacterSettingsLoadedStatus = 0;
            try
            {
                await s_DicLoadedCharacterSettings.ClearAsync(token).ConfigureAwait(false);
                try
                {
                    if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                    {
                        CharacterSettings objNewCharacterSettings = new CharacterSettings();
                        if (!await s_DicLoadedCharacterSettings.TryAddAsync(GlobalSettings.DefaultCharacterSetting,
                                                                            objNewCharacterSettings, token).ConfigureAwait(false))
                            await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                        return;
                    }

                    IEnumerable<XPathNavigator> xmlSettingsIterator
                        = (await (await XmlManager.LoadXPathAsync("settings.xml", token: token).ConfigureAwait(false))
                                 .SelectAndCacheExpressionAsync("/chummer/settings/setting", token: token).ConfigureAwait(false)).Cast<XPathNavigator>();
                    await Task.Run(() =>
                                       Parallel.ForEach(xmlSettingsIterator, xmlBuiltInSetting =>
                                       {
                                           CharacterSettings objNewCharacterSettings = new CharacterSettings();
                                           if (!objNewCharacterSettings.Load(xmlBuiltInSetting)
                                               || (objNewCharacterSettings.BuildMethodIsLifeModule
                                                   && !GlobalSettings.LifeModuleEnabled)
                                               || !s_DicLoadedCharacterSettings.TryAdd(
                                                   objNewCharacterSettings.DictionaryKey,
                                                   objNewCharacterSettings))
                                               objNewCharacterSettings.Dispose();
                                       }), token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Increment(ref _intDicLoadedCharacterSettingsLoadedStatus);
                }

                await Task.Run(() =>
                {
                    string strSettingsPath = Path.Combine(Utils.GetStartupPath, "settings");
                    if (Directory.Exists(strSettingsPath))
                    {
                        Parallel.ForEach(Directory.EnumerateFiles(strSettingsPath, "*.xml"), strSettingsFilePath =>
                        {
                            string strSettingName = Path.GetFileName(strSettingsFilePath);
                            CharacterSettings objNewCharacterSettings = new CharacterSettings();
                            if (!objNewCharacterSettings.Load(strSettingName, false)
                                || (objNewCharacterSettings.BuildMethodIsLifeModule
                                    && !GlobalSettings.LifeModuleEnabled)
                                || !s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                        objNewCharacterSettings))
                                objNewCharacterSettings.Dispose();
                        });
                    }
                }, token).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Increment(ref _intDicLoadedCharacterSettingsLoadedStatus);
            }
        }

        private static async ValueTask AddSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            if (!await objNewCharacterSettings.LoadAsync(strSettingName, false, token).ConfigureAwait(false)
                || (objNewCharacterSettings.BuildMethodIsLifeModule
                    && !GlobalSettings.LifeModuleEnabled))
            {
                await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                return;
            }

            string strKey = await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
            while (true)
            {
                if (await s_DicLoadedCharacterSettings.TryAddAsync(strKey, objNewCharacterSettings, token).ConfigureAwait(false))
                {
                    return;
                }
                // We somehow already have a setting loaded with this name, so just copy over the values and dispose of the new setting instead
                (bool blnSuccess, CharacterSettings objOldCharacterSettings)
                    = await s_DicLoadedCharacterSettings.TryGetValueAsync(strKey, token).ConfigureAwait(false);
                if (blnSuccess)
                {
                    await objOldCharacterSettings.CopyValuesAsync(objNewCharacterSettings, token: token).ConfigureAwait(false);
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    return;
                }
            }
        }

        private static async ValueTask RemoveSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objSettingsToDelete
                = (await s_DicLoadedCharacterSettings.FirstOrDefaultAsync(x => x.Value.FileName == strSettingName, token: token).ConfigureAwait(false)).Value;
            if (objSettingsToDelete == default)
                return;
            string strKeyToDelete = await objSettingsToDelete.GetDictionaryKeyAsync(token).ConfigureAwait(false);

            try
            {
                Lazy<ValueTask<string>> strBestMatchNewSettingsKey = new Lazy<ValueTask<string>>(async () =>
                {
                    int intBestScore = int.MinValue;
                    string strReturn = string.Empty;
                    foreach (CharacterSettings objExistingSettings in await s_DicLoadedCharacterSettings.GetValuesAsync(token).ConfigureAwait(false))
                    {
                        string strLoopKey = await objExistingSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                        if (strKeyToDelete == strLoopKey)
                            continue;
                        // ReSharper disable once AccessToDisposedClosure
                        int intLoopScore = await CalculateCharacterSettingsMatchScore(objSettingsToDelete, objExistingSettings, token).ConfigureAwait(false);
                        if (intLoopScore > intBestScore)
                        {
                            intBestScore = intLoopScore;
                            strReturn = strLoopKey;
                        }
                    }
                    return strReturn;
                });
                await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                {
                    if (await objCharacter.GetSettingsKeyAsync(token).ConfigureAwait(false) == strKeyToDelete)
                        await objCharacter.SetSettingsKeyAsync(await strBestMatchNewSettingsKey.Value.ConfigureAwait(false), token).ConfigureAwait(false);
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                await s_DicLoadedCharacterSettings.RemoveAsync(strKeyToDelete, token).ConfigureAwait(false);
                await objSettingsToDelete.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static async ValueTask ReloadSpecificCustomCharacterSetting(string strSettingName, CancellationToken token = default)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            if (!await objNewCharacterSettings.LoadAsync(strSettingName, false, token).ConfigureAwait(false)
                || (objNewCharacterSettings.BuildMethodIsLifeModule
                    && !GlobalSettings.LifeModuleEnabled))
            {
                await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                return;
            }

            string strKey = await objNewCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
            while (true)
            {
                (bool blnSuccess, CharacterSettings objOldCharacterSettings)
                    = await s_DicLoadedCharacterSettings.TryGetValueAsync(strKey, token).ConfigureAwait(false);
                if (blnSuccess)
                {
                    await objOldCharacterSettings.CopyValuesAsync(objNewCharacterSettings, token: token).ConfigureAwait(false);
                    await objNewCharacterSettings.DisposeAsync().ConfigureAwait(false);
                    return;
                }

                // We ended up changing our dictionary key, so find the first custom setting without a corresponding file and delete it
                // (we assume that it's the one that got renamed)
                if (await s_DicLoadedCharacterSettings.TryAddAsync(strKey, objNewCharacterSettings, token).ConfigureAwait(false))
                {
                    foreach (CharacterSettings objExistingSettings in (await s_DicLoadedCharacterSettings.GetValuesAsync(token).ConfigureAwait(false)).ToList())
                    {
                        if (await objExistingSettings.GetBuiltInOptionAsync(token).ConfigureAwait(false))
                            continue;
                        if (!File.Exists(Path.Combine(Utils.GetStartupPath, "settings", await objExistingSettings.GetFileNameAsync(token).ConfigureAwait(false))))
                        {
                            string strKeyToDelete = await objExistingSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                            await Program.OpenCharacters.ForEachAsync(async objCharacter =>
                            {
                                if (await objCharacter.GetSettingsKeyAsync(token).ConfigureAwait(false) == strKeyToDelete)
                                    await objCharacter.SetSettingsKeyAsync(strKey, token).ConfigureAwait(false);
                            }, token: token).ConfigureAwait(false);
                            await s_DicLoadedCharacterSettings.RemoveAsync(objExistingSettings.DictionaryKey, token).ConfigureAwait(false);
                            await objExistingSettings.DisposeAsync().ConfigureAwait(false);
                            return;
                        }
                    }
                    break;
                }
            }
        }

        private static async ValueTask<int> CalculateCharacterSettingsMatchScore(CharacterSettings objBaselineSettings, CharacterSettings objOptionsToCheck, CancellationToken token = default)
        {
            int intReturn = int.MaxValue - ((await objBaselineSettings.GetBuildKarmaAsync(token).ConfigureAwait(false) - await objOptionsToCheck.GetBuildKarmaAsync(token).ConfigureAwait(false)).RaiseToPower(2)
                                            + (await objBaselineSettings.GetNuyenMaximumBPAsync(token).ConfigureAwait(false) - await objOptionsToCheck.GetNuyenMaximumBPAsync(token).ConfigureAwait(false))
                                            .RaiseToPower(2))
                                           .RaiseToPower(0.5m).StandardRound();
            int intBaseline = await objOptionsToCheck.GetBuiltInOptionAsync(token).ConfigureAwait(false) ? 5 : 4;
            CharacterBuildMethod eLeftBuildMethod = await objBaselineSettings.GetBuildMethodAsync(token).ConfigureAwait(false);
            CharacterBuildMethod eRightBuildMethod = await objOptionsToCheck.GetBuildMethodAsync(token).ConfigureAwait(false);
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

            int intBaselineCustomDataCount = objOptionsToCheck.EnabledCustomDataDirectoryInfos.Count;
            if (intBaselineCustomDataCount == 0)
            {
                intBaselineCustomDataCount = objBaselineSettings.EnabledCustomDataDirectoryInfos.Count;
                if (intBaselineCustomDataCount > 0)
                {
                    intReturn -= intBaselineCustomDataCount.RaiseToPower(2) * intBaseline;
                }
            }
            else if (objBaselineSettings.EnabledCustomDataDirectoryInfos.Count == 0)
            {
                intReturn -= intBaselineCustomDataCount.RaiseToPower(2) * intBaseline;
            }
            else
            {
                intBaselineCustomDataCount
                    = Math.Max(objBaselineSettings.EnabledCustomDataDirectoryInfos.Count,
                               intBaselineCustomDataCount);
                for (int i = 0;
                     i < objOptionsToCheck.EnabledCustomDataDirectoryInfos.Count;
                     ++i)
                {
                    string strLoopCustomDataName =
                        objOptionsToCheck.EnabledCustomDataDirectoryInfos[i].Name;
                    int intLoopIndex =
                        objBaselineSettings.EnabledCustomDataDirectoryInfos.FindIndex(x => x.Name == strLoopCustomDataName);
                    if (intLoopIndex < 0)
                        intReturn -= intBaselineCustomDataCount * intBaseline;
                    else
                        intReturn -= Math.Abs(i - intLoopIndex) * intBaseline;
                }

                foreach (string strLoopCustomDataName in objBaselineSettings.EnabledCustomDataDirectoryInfos.Select(x => x.Name))
                {
                    if (objOptionsToCheck.EnabledCustomDataDirectoryInfos.All(x => x.Name != strLoopCustomDataName))
                        intReturn -= intBaselineCustomDataCount * intBaseline;
                }
            }

            using (new FetchSafelyFromPool<HashSet<string>>(
                       Utils.StringHashSetPool, out HashSet<string> setDummyBooks))
            {
                setDummyBooks.AddRange(objBaselineSettings.Books);
                int intExtraBooks = 0;
                foreach (string strBook in objOptionsToCheck.Books)
                {
                    if (setDummyBooks.Remove(strBook))
                        ++intExtraBooks;
                }
                setDummyBooks.IntersectWith(objOptionsToCheck.Books);
                intReturn -= (setDummyBooks.Count * (intBaselineCustomDataCount + 1)
                              + intExtraBooks) * intBaseline;
            }

            return intReturn;
        }
    }
}
