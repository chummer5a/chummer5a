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

        private static void ObjSettingsFolderWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            using (CursorWait.New())
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        AddSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath));
                        break;
                    case WatcherChangeTypes.Deleted:
                        RemoveSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath));
                        break;
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Renamed:
                        ReloadSpecificCustomCharacterSetting(Path.GetFileName(e.FullPath));
                        break;
                }
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static IAsyncReadOnlyDictionary<string, CharacterSettings> LoadedCharacterSettings
        {
            get
            {
                if (_intDicLoadedCharacterSettingsLoadedStatus < 0) // Makes sure if we end up calling this from multiple threads, only one does loading at a time
                    LoadCharacterSettings();
                while (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                    Utils.SafeSleep();
                return s_DicLoadedCharacterSettings;
            }
        }

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static LockingDictionary<string, CharacterSettings> LoadedCharacterSettingsAsModifiable
        {
            get
            {
                if (_intDicLoadedCharacterSettingsLoadedStatus < 0) // Makes sure if we end up calling this from multiple threads, only one does loading at a time
                    LoadCharacterSettings();
                while (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                    Utils.SafeSleep();
                return s_DicLoadedCharacterSettings;
            }
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
                            = (await (await XmlManager.LoadXPathAsync("settings.xml"))
                                .SelectAndCacheExpressionAsync("/chummer/settings/setting")).Cast<XPathNavigator>();
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

        private static void AddSpecificCustomCharacterSetting(string strSettingName)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            if (!objNewCharacterSettings.Load(strSettingName, false)
                || (objNewCharacterSettings.BuildMethodIsLifeModule
                    && !GlobalSettings.LifeModuleEnabled))
            {
                objNewCharacterSettings.Dispose();
                return;
            }

            while (true)
            {
                if (s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                        objNewCharacterSettings))
                {
                    return;
                }
                // We somehow already have a setting loaded with this name, so just copy over the values and dispose of the new setting instead
                if (s_DicLoadedCharacterSettings.TryGetValue(objNewCharacterSettings.DictionaryKey,
                                                             out CharacterSettings objOldCharacterSettings))
                {
                    objOldCharacterSettings.CopyValues(objNewCharacterSettings);
                    objNewCharacterSettings.Dispose();
                    return;
                }
            }
        }

        private static void RemoveSpecificCustomCharacterSetting(string strSettingName)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objSettingsToDelete
                = s_DicLoadedCharacterSettings.FirstOrDefault(x => x.Value.FileName == strSettingName).Value;
            if (objSettingsToDelete == default)
                return;

            try
            {
                Lazy<string> strBestMatchNewSettingsKey = new Lazy<string>(() =>
                {
                    int intBestScore = int.MinValue;
                    string strReturn = string.Empty;
                    foreach (CharacterSettings objExistingSettings in s_DicLoadedCharacterSettings.Values)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        if (objSettingsToDelete.DictionaryKey == objExistingSettings.DictionaryKey)
                            continue;
                        // ReSharper disable once AccessToDisposedClosure
                        int intLoopScore = CalculateCharacterSettingsMatchScore(objSettingsToDelete, objExistingSettings);
                        if (intLoopScore > intBestScore)
                        {
                            intBestScore = intLoopScore;
                            strReturn = objExistingSettings.DictionaryKey;
                        }
                    }
                    return strReturn;
                });
                foreach (Character objCharacter in Program.OpenCharacters)
                {
                    if (objCharacter.SettingsKey == objSettingsToDelete.DictionaryKey)
                        objCharacter.SettingsKey = strBestMatchNewSettingsKey.Value;
                }
            }
            finally
            {
                s_DicLoadedCharacterSettings.Remove(objSettingsToDelete.DictionaryKey);
                objSettingsToDelete.Dispose();
            }
        }

        private static void ReloadSpecificCustomCharacterSetting(string strSettingName)
        {
            if (_intDicLoadedCharacterSettingsLoadedStatus <= 1)
                return;

            CharacterSettings objNewCharacterSettings = new CharacterSettings();
            if (!objNewCharacterSettings.Load(strSettingName, false)
                || (objNewCharacterSettings.BuildMethodIsLifeModule
                    && !GlobalSettings.LifeModuleEnabled))
            {
                objNewCharacterSettings.Dispose();
                return;
            }

            while (true)
            {
                if (s_DicLoadedCharacterSettings.TryGetValue(objNewCharacterSettings.DictionaryKey,
                                                             out CharacterSettings objOldCharacterSettings))
                {
                    objOldCharacterSettings.CopyValues(objNewCharacterSettings);
                    objNewCharacterSettings.Dispose();
                    return;
                }

                // We ended up changing our dictionary key, so find the first custom setting without a corresponding file and delete it
                // (we assume that it's the one that got renamed)
                if (s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                        objNewCharacterSettings))
                {
                    foreach (CharacterSettings objExistingSettings in s_DicLoadedCharacterSettings.Values.ToList())
                    {
                        if (objExistingSettings.BuiltInOption)
                            continue;
                        if (!File.Exists(Path.Combine(Utils.GetStartupPath, "settings", objExistingSettings.FileName)))
                        {
                            foreach (Character objCharacter in Program.OpenCharacters)
                            {
                                if (objCharacter.SettingsKey == objExistingSettings.DictionaryKey)
                                    objCharacter.SettingsKey = objNewCharacterSettings.DictionaryKey;
                            }
                            s_DicLoadedCharacterSettings.Remove(objExistingSettings.DictionaryKey);
                            objExistingSettings.Dispose();
                            return;
                        }
                    }
                    break;
                }
            }
        }

        private static void LoadCustomCharacterSettings()
        {
            // Don't attempt to load custom character settings if we're still loading all settings
            if (Interlocked.CompareExchange(ref _intDicLoadedCharacterSettingsLoadedStatus, 1, 2) <= 1)
                return;
            try
            {
                using (LockingDictionary<string, CharacterSettings> dicNewLoadedCharacterSettings = new LockingDictionary<string, CharacterSettings>())
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
                                // ReSharper disable once AccessToDisposedClosure
                                || !dicNewLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                                                         objNewCharacterSettings))
                                objNewCharacterSettings.Dispose();
                        });
                    }

                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> setRemovedSettingsKeys))
                    {
                        foreach (CharacterSettings objExistingSettings in s_DicLoadedCharacterSettings.Values.ToList())
                        {
                            if (objExistingSettings.BuiltInOption)
                                continue;
                            if (!dicNewLoadedCharacterSettings.TryRemove(objExistingSettings.DictionaryKey,
                                                                         out CharacterSettings objNewSettings))
                            {
                                setRemovedSettingsKeys.Add(objExistingSettings.DictionaryKey);
                            }
                            else
                            {
                                objExistingSettings.CopyValues(objNewSettings);
                                objNewSettings.Dispose();
                            }
                        }

                        foreach (CharacterSettings objNewSettings in dicNewLoadedCharacterSettings.Values)
                        {
                            if (!s_DicLoadedCharacterSettings.TryAdd(objNewSettings.DictionaryKey, objNewSettings))
                                objNewSettings.Dispose();
                        }

                        foreach (string strSettingToRemove in setRemovedSettingsKeys)
                        {
                            CharacterSettings objSettingsToDelete = s_DicLoadedCharacterSettings[strSettingToRemove];
                            try
                            {
                                Lazy<string> strBestMatchNewSettingsKey = new Lazy<string>(() =>
                                {
                                    int intBestScore = int.MinValue;
                                    string strReturn = string.Empty;
                                    foreach (CharacterSettings objExistingSettings in s_DicLoadedCharacterSettings.Values)
                                    {
                                        if (setRemovedSettingsKeys.Contains(objExistingSettings.DictionaryKey))
                                            continue;
                                        // ReSharper disable once AccessToDisposedClosure
                                        int intLoopScore = CalculateCharacterSettingsMatchScore(objSettingsToDelete, objExistingSettings);
                                        if (intLoopScore > intBestScore)
                                        {
                                            intBestScore = intLoopScore;
                                            strReturn = objExistingSettings.DictionaryKey;
                                        }
                                    }
                                    return strReturn;
                                });
                                foreach (Character objCharacter in Program.OpenCharacters)
                                {
                                    if (objCharacter.SettingsKey == objSettingsToDelete.DictionaryKey)
                                        objCharacter.SettingsKey = strBestMatchNewSettingsKey.Value;
                                }
                            }
                            finally
                            {
                                s_DicLoadedCharacterSettings.Remove(objSettingsToDelete.DictionaryKey);
                                objSettingsToDelete.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Increment(ref _intDicLoadedCharacterSettingsLoadedStatus);
            }
        }

        private static int CalculateCharacterSettingsMatchScore(CharacterSettings objBaselineSettings, CharacterSettings objOptionsToCheck)
        {
            int intBaseline
                = (Convert.ToDecimal((objBaselineSettings.BuildKarma - objOptionsToCheck.BuildKarma).RaiseToPower(2))
                   + (objBaselineSettings.NuyenMaximumBP - objOptionsToCheck.NuyenMaximumBP).RaiseToPower(2))
                  .RaiseToPower(0.5m).StandardRound();
            if (objOptionsToCheck.BuiltInOption)
                ++intBaseline;

            int intReturn = int.MaxValue;
            if (objOptionsToCheck.BuildMethod != objBaselineSettings.BuildMethod)
            {
                if (objOptionsToCheck.BuildMethod.UsesPriorityTables() ==
                    objBaselineSettings.BuildMethod.UsesPriorityTables())
                    intReturn -= intBaseline.RaiseToPower(2) / 2;
                else
                    intReturn -= intBaseline.RaiseToPower(2);
            }

            int intBaselineCustomDataCount = Math.Max(objOptionsToCheck.EnabledCustomDataDirectoryInfos.Count,
                                                      objBaselineSettings.EnabledCustomDataDirectoryInfos.Count);
            for (int i = 0;
                 i < objOptionsToCheck.EnabledCustomDataDirectoryInfos.Count;
                 ++i)
            {
                string strLoopCustomDataName =
                    objOptionsToCheck.EnabledCustomDataDirectoryInfos[i].Name;
                int intLoopIndex =
                    objBaselineSettings.EnabledCustomDataDirectoryInfos.FindIndex(x => x.Name == strLoopCustomDataName);
                if (intLoopIndex < 0)
                    intReturn -= intBaselineCustomDataCount.RaiseToPower(2) * intBaseline;
                else
                    intReturn -= (i - intLoopIndex).RaiseToPower(2) * intBaseline;
            }

            foreach (string strLoopCustomDataName in objBaselineSettings.EnabledCustomDataDirectoryInfos.Select(x => x.Name))
            {
                if (objOptionsToCheck.EnabledCustomDataDirectoryInfos.All(x => x.Name != strLoopCustomDataName))
                    intReturn -= intBaselineCustomDataCount.RaiseToPower(2) * intBaseline;
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
                intReturn -= (setDummyBooks.Count.RaiseToPower(2) * (intBaselineCustomDataCount + 1)
                              + intExtraBooks.RaiseToPower(2)) * intBaseline;
            }

            return intReturn;
        }
    }
}
