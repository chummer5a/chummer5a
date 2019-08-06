using System;
using System.Collections.Generic;
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
    static class GlobalOptions
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
        public static List<MRUEntry> MostRecentlyUsedList { get; } = new List<MRUEntry>();

        private const string PROGRAM_SETTINGS_FILE = "programdata.local.xml";
        private static List<CharacterOptions> fileOptions;
        private static CharacterOptions _default;

        #region MRU Methods

        public static event EventHandler<TextEventArgs> MRUChanged;


        public static void MRUAdd(string entryPath)
        {
            int progress;
            for (progress = 0; progress < 10; progress++)
            {
                if (MostRecentlyUsedList[progress].Path == entryPath)
                {
                    return; //Found in sticky
                }

                //If not sticky anymore, need different logic
                //Checking sticky after path. This means first after sticky returns above.
                //This is fine, as refreshing top needs no action
                if (!MostRecentlyUsedList[progress].Sticky) break;
            }

            int topOfSticky = progress;
            int index = MostRecentlyUsedList.FindIndex(topOfSticky, mru => mru.Path == entryPath);

            MRUEntry entry;
            if (index == -1) //Not found
            {
                entry = new MRUEntry(entryPath);
                MostRecentlyUsedList.Insert(topOfSticky, entry);

                //Remove every over index 9, backwards from performance (hah) reasons
                //I don't actually think there can be more than 10 entries atm, but not much more complicated than if
                //What off by one error?
                for (int i = MostRecentlyUsedList.Count - 1; i >= 9; i--)
                {
                    MostRecentlyUsedList.RemoveAt(i);
                }
            }
            //Found
            else
            {
                //Move to top and rotate down
                entry = MostRecentlyUsedList[index];
                MostRecentlyUsedList.RemoveAt(index);
                MostRecentlyUsedList.Insert(topOfSticky, entry);
            }

            MRUChanged?.Invoke(entry, new TextEventArgs("mru"));
            //Needs to handle
            //Item already in list
            //New item
            //Stickies
            //Full stickies
        }

        public static void MruToggleSticky(MRUEntry entry)
        {
            //I'm sure this can be changed to a generalized case, but i can't see how right now
            if (entry.Sticky)
            {
                int newIndex = MostRecentlyUsedList.FindIndex(x => !x.Sticky);
                int oldIndex = MostRecentlyUsedList.IndexOf(entry);
                entry.Sticky = false;
                MostRecentlyUsedList.RemoveAt(oldIndex);
                MostRecentlyUsedList.Insert(newIndex - 1, entry);
            }
            else
            {
                int newIndex = MostRecentlyUsedList.FindLastIndex(x => x.Sticky);
                int oldIndex = MostRecentlyUsedList.IndexOf(entry);
                entry.Sticky = true;
                MostRecentlyUsedList.RemoveAt(oldIndex);
                MostRecentlyUsedList.Insert(newIndex + 1, entry);

            }


            //Intentionally swapped, I since sticky has changed
            MRUChanged?.Invoke(entry, new TextEventArgs(entry.Sticky ? "mru" : "mrusticky"));


        }

        /// <summary>
        /// Retrieve the list of most recently used characters.
        /// </summary>
        private static List<string> ReadMRUList()
        {
            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            List<string> lstFiles = new List<string>();

            for (int i = 1; i <= 10; i++)
            {
                if ((objRegistry.GetValue("mru" + i.ToString())) != null)
                {
                    lstFiles.Add(objRegistry.GetValue("mru" + i.ToString()).ToString());
                }
            }

            return lstFiles;
        }

        /// <summary>
        /// Retrieve the list of sticky most recently used characters.
        /// </summary>
        private static List<string> ReadStickyMRUList()
        {
            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            List<string> lstFiles = new List<string>();

            for (int i = 1; i <= 10; i++)
            {
                if ((objRegistry.GetValue("stickymru" + i.ToString())) != null)
                {
                    lstFiles.Add(objRegistry.GetValue("stickymru" + i.ToString()).ToString());
                }
            }

            return lstFiles;
        }

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
            LoadMRUEntries(xmlOptionDocument);

            fileOptions = LoadCharacterOptionses();

            //TODO: find default characteroption


        }

        private static void LoadMRUEntries([CanBeNull] XmlDocument xmlOptionSource)
        {
            MostRecentlyUsedList.Clear();
            if (Instance.SavedByVersion == null)
            {
                MostRecentlyUsedList.AddRange(ReadStickyMRUList().Select(x => new MRUEntry(x) {Sticky = true}));
                MostRecentlyUsedList.AddRange(ReadMRUList().Select(x => new MRUEntry(x)));
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    MRUEntry entry = new MRUEntry(null);
                    if (xmlOptionSource != null)
                    {
                        XmlNode mruNode = xmlOptionSource["settings"]?["mru"]?["mru" + i];
                        if (mruNode == null) break;
                        ClassSaver.Load(ref entry, mruNode);
                    }
                    else
                    {
                        RegistryKey mruKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Mru\\" + i);
                        if (mruKey == null) break;
                        ClassSaver.Load(ref entry, mruKey);
                    }
                    MostRecentlyUsedList.Add(entry);
                }
            }
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
                SourcebookInfo objSource = new SourcebookInfo(objXmlBook["code"].InnerText, objXmlBook["name"].InnerText); //TODO: Localize
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

        public static List<CharacterOptions> LoadCharacterOptionses()
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
            for (int i = 0; i < MostRecentlyUsedList.Count; i++)
            {
                RegistryKey mruKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Mru\\" + i);
                ClassSaver.Save(MostRecentlyUsedList[i], mruKey);
            }
        }

        private static void SaveMRUToXmlWriter(XmlWriter writer)
        {
            writer.WriteStartElement("mru");
            for (int i = 0; i < MostRecentlyUsedList.Count; i++)
            {
                writer.WriteStartElement("mru" + i);

                ClassSaver.Save(MostRecentlyUsedList[i], writer);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        public static void SaveGlobalOptions()
        {
            Instance.SavedByVersion = Assembly.GetEntryAssembly().GetName().Version;

            if (Utils.IsLinux)
            {
                string globalOptionPath = GlobalOptionLinuxFile();

                Directory.CreateDirectory(Path.GetDirectoryName(globalOptionPath));
                using (FileStream fs = new FileStream(globalOptionPath, FileMode.Create))
                {
                    XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
                    writer.WriteStartElement("settings");

                    ClassSaver.Save(GlobalOptions.Instance, writer);

                    writer.WriteStartElement("books");
                    foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
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
                ClassSaver.Save(GlobalOptions.Instance, rootKey);
                int count = 0;
                RegistryKey bookKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Books");
                foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
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
                "globaloptions.xml"
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
