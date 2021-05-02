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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    public static class OptionsManager
    {
        private static bool s_blnDicLoadedCharacterOptionsLoaded;
        private static readonly ConcurrentDictionary<string, CharacterOptions> s_dicLoadedCharacterOptions = new ConcurrentDictionary<string, CharacterOptions>();

        public static IDictionary<string, CharacterOptions> LoadedCharacterOptions
        {
            get
            {
                if (!s_blnDicLoadedCharacterOptionsLoaded)
                    LoadCharacterOptions();
                return s_dicLoadedCharacterOptions;
            }
        }

        private static void LoadCharacterOptions()
        {
            s_blnDicLoadedCharacterOptionsLoaded = false;
            s_dicLoadedCharacterOptions.Clear();
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
            {
                s_dicLoadedCharacterOptions.TryAdd(GlobalOptions.DefaultCharacterOption, new CharacterOptions());
                return;
            }
            Parallel.ForEach(
                XmlManager.LoadXPath("settings.xml").Select("/chummer/settings/setting").Cast<XPathNavigator>(),
                xmlBuiltInSetting =>
                {
                    CharacterOptions objNewCharacterOptions = new CharacterOptions();
                    if (objNewCharacterOptions.Load(xmlBuiltInSetting) &&
                        (!objNewCharacterOptions.BuildMethodIsLifeModule || GlobalOptions.LifeModuleEnabled))
                        s_dicLoadedCharacterOptions.TryAdd(objNewCharacterOptions.SourceId, objNewCharacterOptions);
                });
            string strSettingsPath = Path.Combine(Utils.GetStartupPath, "settings");
            if (Directory.Exists(strSettingsPath))
            {
                Parallel.ForEach(Directory.EnumerateFiles(strSettingsPath, "*.xml"), strSettingsFilePath =>
                {
                    string strSettingName = Path.GetFileName(strSettingsFilePath);
                    CharacterOptions objNewCharacterOptions = new CharacterOptions();
                    if (objNewCharacterOptions.Load(strSettingName, false) &&
                        (!objNewCharacterOptions.BuildMethodIsLifeModule || GlobalOptions.LifeModuleEnabled))
                        s_dicLoadedCharacterOptions.TryAdd(strSettingName, objNewCharacterOptions);
                });
            }
            s_blnDicLoadedCharacterOptionsLoaded = true;
        }
    }
}
