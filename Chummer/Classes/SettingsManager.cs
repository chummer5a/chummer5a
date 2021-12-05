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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public static class SettingsManager
    {
        private static int _intDicLoadedCharacterSettingsLoadedStatus = -1;
        private static readonly LockingDictionary<string, CharacterSettings> s_DicLoadedCharacterSettings = new LockingDictionary<string, CharacterSettings>();

        // Looks awkward to have two different versions of the same property, but this allows for easier tracking of where character settings are being modified
        public static IReadOnlyDictionary<string, CharacterSettings> LoadedCharacterSettings
        {
            get
            {
                if (_intDicLoadedCharacterSettingsLoadedStatus < 0) // Makes sure if we end up calling this from multiple threads, only one does loading at a time
                    LoadCharacterSettings();
                while (_intDicLoadedCharacterSettingsLoadedStatus <= 0)
                {
                    Utils.SafeSleep();
                }
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
                while (_intDicLoadedCharacterSettingsLoadedStatus <= 0)
                {
                    Utils.SafeSleep();
                }
                return s_DicLoadedCharacterSettings;
            }
        }

        private static void LoadCharacterSettings()
        {
            _intDicLoadedCharacterSettingsLoadedStatus = 0;
            try
            {
                s_DicLoadedCharacterSettings.Clear();
                if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                {
                    s_DicLoadedCharacterSettings.TryAdd(GlobalSettings.DefaultCharacterSetting, new CharacterSettings());
                    return;
                }

                IEnumerable<XPathNavigator> xmlSettingsIterator = XmlManager.LoadXPath("settings.xml")
                    .SelectAndCacheExpression("/chummer/settings/setting").Cast<XPathNavigator>();
                Parallel.ForEach(xmlSettingsIterator, xmlBuiltInSetting =>
                {
                    CharacterSettings objNewCharacterSettings = new CharacterSettings();
                    if (objNewCharacterSettings.Load(xmlBuiltInSetting) &&
                        (!objNewCharacterSettings.BuildMethodIsLifeModule || GlobalSettings.LifeModuleEnabled))
                        s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                            objNewCharacterSettings);
                });
                string strSettingsPath = Path.Combine(Utils.GetStartupPath, "settings");
                if (Directory.Exists(strSettingsPath))
                {
                    Parallel.ForEach(Directory.EnumerateFiles(strSettingsPath, "*.xml"), strSettingsFilePath =>
                    {
                        string strSettingName = Path.GetFileName(strSettingsFilePath);
                        CharacterSettings objNewCharacterSettings = new CharacterSettings();
                        if (objNewCharacterSettings.Load(strSettingName, false) &&
                            (!objNewCharacterSettings.BuildMethodIsLifeModule || GlobalSettings.LifeModuleEnabled))
                            s_DicLoadedCharacterSettings.TryAdd(objNewCharacterSettings.DictionaryKey,
                                objNewCharacterSettings);
                    });
                }
            }
            finally
            {
                _intDicLoadedCharacterSettingsLoadedStatus = 1;
            }
        }
    }
}
