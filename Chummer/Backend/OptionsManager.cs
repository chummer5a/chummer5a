using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Microsoft.Win32;

namespace Chummer
{
    public static class GlobalOptions
    {
        //For the record I totally don't think Options is the right place to throw constants, by their nature of being "constants"
        #region Constants
        public const string DefaultLanguage = "en-us";
        public const string DefaultCharacterSheetDefaultValue = "Shadowrun 5 (Skills grouped by Rating greater 0)";
        public const string DefaultGameplayOption = "Standard";
        public const string DefaultBuildMethod = "Priority";
        public const int MaxMruSize = 10;

        #endregion

        private static ProgramOptions _instance;
        public static ProgramOptions Instance => _instance;

        //TODO: Lazy shim to make it simpler to merge master changes. Revert later.
        public static CultureInfo InvariantCultureInfo => _instance.InvariantCultureInfo;
        public static CultureInfo CultureInfo => _instance.CultureInfo;
        public static string Language => Instance.Language;

        private static readonly MostRecentlyUsedCollection<string> _lstMostRecentlyUsedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);
        private static readonly MostRecentlyUsedCollection<string> _lstFavoritedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);

        private const string PROGRAM_SETTINGS_FILE = "programdata.local.xml";
        private static List<CharacterOptions> fileOptions;
        private static CharacterOptions _default;
        private static readonly RegistryKey _objBaseChummerKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");

        #region MRU Methods

        public static event EventHandler<TextEventArgs> MRUChanged;
        private static void LstFavoritedCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstFavoritedCharacters.Count)
                                _objBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), _lstFavoritedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstFavoritedCharacters.Count)
                                _objBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), _lstFavoritedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strNewFile = e.NewItems.Count > 0 ? e.NewItems[0] as string : string.Empty;
                        if (!string.IsNullOrEmpty(strNewFile))
                            _objBaseChummerKey.SetValue("stickymru" + (e.OldStartingIndex + 1).ToString(InvariantCultureInfo), strNewFile);
                        else
                        {
                            for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if (i <= _lstFavoritedCharacters.Count)
                                    _objBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), _lstFavoritedCharacters[i - 1]);
                                else
                                    _objBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                            }
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        int intOldStartingIndex = e.OldStartingIndex;
                        int intNewStartingIndex = e.NewStartingIndex;
                        if (intOldStartingIndex == intNewStartingIndex)
                            break;

                        int intUpdateFrom;
                        int intUpdateTo;
                        if (intOldStartingIndex > intNewStartingIndex)
                        {
                            intUpdateFrom = intNewStartingIndex;
                            intUpdateTo = intOldStartingIndex;
                        }
                        else
                        {
                            intUpdateFrom = intOldStartingIndex;
                            intUpdateTo = intNewStartingIndex;
                        }

                        for (int i = intUpdateFrom; i <= intUpdateTo; ++i)
                        {
                            _objBaseChummerKey.SetValue("stickymru" + (i + 1).ToString(InvariantCultureInfo), _lstFavoritedCharacters[i]);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstFavoritedCharacters.Count)
                                _objBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), _lstFavoritedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
            }
        }

        private static void LstMostRecentlyUsedCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(), false);
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strNewFile = e.NewItems.Count > 0 ? e.NewItems[0] as string : string.Empty;
                        if (!string.IsNullOrEmpty(strNewFile))
                        {
                            _objBaseChummerKey.SetValue("mru" + (e.OldStartingIndex + 1).ToString(), strNewFile);
                        }
                        else
                        {
                            for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if (i <= _lstMostRecentlyUsedCharacters.Count)
                                    _objBaseChummerKey.SetValue("mru" + i.ToString(), _lstMostRecentlyUsedCharacters[i - 1]);
                                else
                                    _objBaseChummerKey.DeleteValue("mru" + i.ToString(), false);
                            }
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        int intOldStartingIndex = e.OldStartingIndex;
                        int intNewStartingIndex = e.NewStartingIndex;
                        if (intOldStartingIndex == intNewStartingIndex)
                            break;

                        int intUpdateFrom;
                        int intUpdateTo;
                        if (intOldStartingIndex > intNewStartingIndex)
                        {
                            intUpdateFrom = intNewStartingIndex;
                            intUpdateTo = intOldStartingIndex;
                        }
                        else
                        {
                            intUpdateFrom = intOldStartingIndex;
                            intUpdateTo = intNewStartingIndex;
                        }

                        for (int i = intUpdateFrom; i <= intUpdateTo; ++i)
                        {
                            _objBaseChummerKey.SetValue("mru" + (i + 1).ToString(), _lstMostRecentlyUsedCharacters[i]);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
            }
        }

        public static ObservableCollection<string> FavoritedCharacters => _lstFavoritedCharacters;

        public static ObservableCollection<string> MostRecentlyUsedCharacters => _lstMostRecentlyUsedCharacters;
        #endregion

        public static void Load()
        {
            
            if (!Utils.IsRunningInVisualStudio)
            {
                string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
                if (!Directory.Exists(settingsDirectoryPath))
                    Directory.CreateDirectory(settingsDirectoryPath);
            }

            XmlDocument xmlOptionDocument = null;

            if (Utils.IsLinux)
            {
                xmlOptionDocument = new XmlDocument();
                xmlOptionDocument.Load(GlobalOptionLinuxFile());
            }
            else
            {

            }

            LoadGlobalOptions(xmlOptionDocument);

            //Load MRU
            LoadMRUEntries();

            fileOptions = LoadCharacterOptions();

            //TODO: find default characteroption


        }

        private static void LoadMRUEntries()
        {
            for (int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue("stickymru" + i.ToString(InvariantCultureInfo));
                if (objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if (File.Exists(strFileName) && !_lstFavoritedCharacters.Contains(strFileName))
                        _lstFavoritedCharacters.Add(strFileName);
                }
            }
            _lstFavoritedCharacters.CollectionChanged += LstFavoritedCharactersOnCollectionChanged;

            for (int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue("mru" + i.ToString(InvariantCultureInfo));
                if (objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if (File.Exists(strFileName) && !_lstMostRecentlyUsedCharacters.Contains(strFileName))
                        _lstMostRecentlyUsedCharacters.Add(strFileName);
                }
            }
            _lstMostRecentlyUsedCharacters.CollectionChanged += LstMostRecentlyUsedCharactersOnCollectionChanged;
        }

        private static void LoadGlobalOptions([CanBeNull] XmlDocument xmlOptionSource)
        {
            _instance = new ProgramOptions();
            bool fuckload = false;
            try
            {
                if (xmlOptionSource != null)
                {
                    ClassSaver.Load(ref _instance, xmlOptionSource);
                }
                else
                {
                    RegistryKey rootKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
                    ClassSaver.Load(ref _instance, rootKey);
                }
            }
            catch (Exception)
            {
                fuckload = true;
            }

            // Retrieve the SourcebookInfo objects.
            XmlNodeList objXmlBookList = XmlManager.Load("books.xml").SelectNodes("/chummer/books/book");
            foreach (XmlNode objXmlBook in objXmlBookList)
            {
                SourcebookInfo objSource = new SourcebookInfo(objXmlBook["code"].InnerText, objXmlBook["name"].InnerText);
                _instance.SourcebookInfo.Add(objSource);
            }

            if (!fuckload)
            {

                try
                {
                    if (xmlOptionSource != null)
                    {
                        foreach (XmlNode book in xmlOptionSource["settings"]["books"].ChildNodes)
                        {
                            string bookCode = book["book"].InnerText;
                            SourcebookInfo info = _instance.SourcebookInfo.First(x => x.Code == bookCode);
                            if (info != null)
                                ClassSaver.Load(ref info, book);
                        }
                    }
                    else
                    {
                        RegistryKey bookKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Books");
                        var z = bookKey.GetSubKeyNames();
                        foreach (RegistryKey specificBook in z
                            .Select(x => bookKey.OpenSubKey(x)))
                        {
                            string bookCode = specificBook.GetValue("code").ToString();
                            SourcebookInfo info = _instance.SourcebookInfo.FirstOrDefault(x => x.Code == bookCode);
                            if (info != null)
                                ClassSaver.Load(ref info, specificBook);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static List<CharacterOptions> LoadCharacterOptions()
        {
            //todo: load globaloptions from somewhere (registry or file (LATER))


            List<CharacterOptions> loadedFiles = new List<CharacterOptions>();

            foreach (string file in Directory.EnumerateFiles(Path.Combine(Application.StartupPath, "settings")))
            {
                string filename = Path.GetFileName(file);
                if (filename == PROGRAM_SETTINGS_FILE) continue;

                CharacterOptions o = new CharacterOptions(filename);
                XmlDocument document = new XmlDocument();
                document.Load(file);

                if (document.DocumentElement.Name != "settings")
                {
                    Log.Error($"Option file without <settings>? {file}");
                    continue;
                }

                if (Version.Parse(document.DocumentElement.Attributes["fileversion"]?.Value ?? "0.0.0") <= new Version(5, 190, 0))
                {
                    //Old settings files tries to keep stuff in categories for ?reasons? but new autogenerated just dumps
                    //everything in the node it gets assigned. Move a few things to root node so it can be read

                    MoveToRoot(document, "karmacost");
                    MoveToRoot(document, "defaultbuild");
                    MoveToRoot(document, "bpcost");
                }

                ClassSaver.Load(ref o, document.DocumentElement);

                foreach (string book in o.Books.Keys.ToList())
                {
                    o.Books[book] = false;
                }
                foreach (XmlNode node in document.DocumentElement["books"].ChildNodes)
                {
                    o.Books[node.InnerText] = true;
                }


                loadedFiles.Add(o);
            }

            if (loadedFiles.Count == 0)
                loadedFiles.Add(new CharacterOptions("default.xml"));


            return loadedFiles;
        }

        public static void SaveMRUEntries()
        {
            if (Utils.IsLinux)
            {
                XmlDocument document = new XmlDocument();
                if(File.Exists(GlobalOptionLinuxFile()))
                { document.Load(GlobalOptionLinuxFile());}


                if (document["settings"] == null)
                {
                    document.AppendChild(document.CreateElement("settings"));
                }
                

                XmlDocument fooDoc = new XmlDocument();
                using (XmlWriter writer = fooDoc.CreateNavigator().AppendChild())
                {
                    SaveMRUToXmlWriter(writer);

                }

                if (document["settings"]?["mru"] != null)
                {
                    document["settings"].ReplaceChild(document["settings"]["mru"], document.ImportNode(fooDoc.FirstChild, true));
                }
                else
                {
                    document["settings"].AppendChild(document.ImportNode(fooDoc.FirstChild,true));
                }

                Directory.CreateDirectory(Path.GetDirectoryName(GlobalOptionLinuxFile()));
                document.Save(GlobalOptionLinuxFile());
            }
            else
            {
                SaveMRUToRegistry();
            }
        }

        private static void SaveMRUToRegistry()
        {
            for (int i = 0; i < MostRecentlyUsedCharacters.Count; i++)
            {
                RegistryKey mruKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Mru\\" + i);
                ClassSaver.Save(MostRecentlyUsedCharacters[i], mruKey);
            }
            for (int i = 0; i < FavoritedCharacters.Count; i++)
            {
                RegistryKey mruKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\StickyMru\\" + i);
                ClassSaver.Save(FavoritedCharacters[i], mruKey);
            }
        }

        private static void SaveMRUToXmlWriter(XmlWriter writer)
        {
            writer.WriteStartElement("mru");
            for (int i = 0; i < MostRecentlyUsedCharacters.Count; i++)
            {
                writer.WriteStartElement("mru" + i);

                ClassSaver.Save(MostRecentlyUsedCharacters[i], writer);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        public static void SaveGlobalOptions()
        {
            Instance.SavedByVersion = Assembly.GetEntryAssembly()?.GetName().Version;

            if (Utils.IsLinux)
            {
                string globalOptionPath = GlobalOptionLinuxFile();

                Directory.CreateDirectory(Path.GetDirectoryName(globalOptionPath));
                using (FileStream fs = new FileStream(globalOptionPath, FileMode.Create))
                {
                    XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
                    writer.WriteStartElement("settings");

                    ClassSaver.Save(Instance, writer);

                    writer.WriteStartElement("books");
                    foreach (SourcebookInfo book in Instance.SourcebookInfo)
                    {
                        writer.WriteStartElement("book");
                        ClassSaver.Save(book, writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.Flush();
                    fs.Flush();
                }
            }
            else
            {
                RegistryKey rootKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
                ClassSaver.Save(Instance, rootKey);
                int count = 0;
                RegistryKey bookKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Books");
                foreach (SourcebookInfo book in Instance.SourcebookInfo)
                {
                    RegistryKey k2 = bookKey.CreateSubKey(count.ToString("D2"));
                    ClassSaver.Save(book, k2);
                    count++;
                }

                SaveMRUToRegistry();
            }
        }

        public static void SaveCharacterOption(CharacterOptions options)
        {

            //TODO: SavedByVersion
            string optionPath = Path.Combine(SettingsDirectory(), options.FileName);
            


            Directory.CreateDirectory(Path.GetDirectoryName(optionPath));
            using (FileStream fs = new FileStream(optionPath, FileMode.Create))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("fileversion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                ClassSaver.Save(options, writer);
                writer.WriteStartElement("books");
                foreach (var book in options.Books.Where(x => x.Value).Select(x => x.Key))
                {
                    writer.WriteStartElement("book");
                    writer.WriteElementString("book", book);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                fs.Flush();
            }
        }

        private static string GlobalOptionLinuxFile()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", 
                "Chummer5a", 
                "GlobalOptions.Instance.xml"
            );
        }

        private static string SettingsDirectory()
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "settings");
        }

        private static void MoveToRoot(XmlDocument document, string child)
        {
            XmlNode n = document.DocumentElement[child];
            if(n == null) return;

            document.DocumentElement.RemoveChild(n);
            while (n.HasChildNodes)
            {
                document.DocumentElement.AppendChild(n.FirstChild);
            }
        }

        public static  CharacterOptions Default => _default ?? (_default = fileOptions[0]); //TODO:Add and read from globaloptions

        public static IEnumerable<CharacterOptions> LoadedOptions()
        {
            foreach (CharacterOptions characterOptionse in fileOptions)
            {
                yield return characterOptionse;
            }
            
        }
    }
}
