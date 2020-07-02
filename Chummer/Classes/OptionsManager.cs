using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
                {
                    LoadCharacterOptions();
                    s_blnDicLoadedCharacterOptionsLoaded = true;
                }

                return s_dicLoadedCharacterOptions;
            }
        }

        private static void LoadCharacterOptions()
        {
            s_dicLoadedCharacterOptions.Clear();
            foreach (XPathNavigator xmlBuiltInSetting in XmlManager.LoadXPath("settings.xml").CreateNavigator().Select("/chummer/settings/setting"))
            {
                CharacterOptions objNewCharacterOptions = new CharacterOptions();
                if (objNewCharacterOptions.Load(xmlBuiltInSetting))
                    s_dicLoadedCharacterOptions.TryAdd(objNewCharacterOptions.SourceId, objNewCharacterOptions);
            }
            foreach (string strSettingsFilePath in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "settings"), "*.xml"))
            {
                CharacterOptions objNewCharacterOptions = new CharacterOptions();
                if (objNewCharacterOptions.Load(strSettingsFilePath, false))
                    s_dicLoadedCharacterOptions.TryAdd(strSettingsFilePath, objNewCharacterOptions);
            }
        }
    }
}
