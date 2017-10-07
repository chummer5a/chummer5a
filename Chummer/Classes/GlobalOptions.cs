using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Attributes.SaveAttributes;
using Chummer.Backend.Equipment;
using Chummer.Classes;
using Microsoft.Win32;

namespace Chummer
{
    /// <summary>
    /// Global Options. A single instance class since Options are common for all characters, reduces execution time and memory usage.
    /// </summary>
    public sealed class GlobalOptions
    {
        public event MRUChangedHandler MRUChanged;

        // Omae Information.

        public static GradeList CyberwareGrades = new GradeList();
        public static GradeList BiowareGrades = new GradeList();
        private static readonly GlobalOptions _instance;

        // PDF information.

        #region Constructor and Instance

        static GlobalOptions()
        {
            if (!Utils.IsRunningInVisualStudio())
            {
                string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
                if (!Directory.Exists(settingsDirectoryPath))
                    Directory.CreateDirectory(settingsDirectoryPath);
            }

            bool fuckload = false;
            _instance = new GlobalOptions();
            ClassSaver loader = new ClassSaver();
            XmlDocument doc = new XmlDocument();
            try
            {
                if (Utils.IsLinux)
                {

                    doc.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".config", "Chummer5a", "globaloptions.xml"));

                    loader.Load(ref _instance, doc.ParentNode);

                }
                else
                {
                    RegistryKey rootKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
                    loader.Load(ref _instance, rootKey);

                    //TODO: Temporary readMRU for old options (Move this)
                    _instance.MostRecentlyUsedList.AddRange(ReadStickyMRUList().Select(x => new MRUEntry(x) {Sticky = true}));
                    _instance.MostRecentlyUsedList.AddRange(ReadMRUList().Select(x => new MRUEntry(x)));
                }
            }
            catch
            {
                fuckload = true;
            }

            // Retrieve the SourcebookInfo objects.
            XmlDocument objXmlDocument = XmlManager.Instance.Load("books.xml");
            XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
            foreach (XmlNode objXmlBook in objXmlBookList)
            {
                SourcebookInfo objSource = new SourcebookInfo(objXmlBook["code"].InnerText, objXmlBook["name"].InnerText); //TODO: Localize
                _instance.SourcebookInfo.Add(objSource);

            }

            if (!fuckload)
            {
                try
                {
                    if (Utils.IsLinux)
                    {
                        foreach (XmlNode book in doc["settings"]["books"].ChildNodes)
                        {
                            string bookCode = book["book"].InnerText;
                            SourcebookInfo info = _instance.SourcebookInfo.First(x => x.Code == bookCode);
                            if (info != null)
                                loader.Load(ref info, book);
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
                                loader.Load(ref info, specificBook);
                        }
                    }
                }
                catch {}
            }

            CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware);
            BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware);
        }

        private GlobalOptions()
        {



        }

        /// <summary>
        /// Global instance of the GlobalOptions.
        /// </summary>
        public static GlobalOptions Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Properties
        [OptionAttributes("OptionHeader_GlobalOptions")]
        /// <summary>
        /// Whether or not Automatic Updates are enabled.
        /// </summary>
        [SavePropertyAs("autoupdate")]
        public bool AutomaticUpdate { get; set; }

        [SavePropertyAs("lifemodule")]
        public bool LifeModuleEnabled { get; set; }

        /// <summary>
        /// Whether or not the app should only download localised files in the user's selected language.
        /// </summary>
        [SavePropertyAs("localisedupdatesonly")]
        public bool LocalisedUpdatesOnly { get; set; } = false;

        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        [SavePropertyAs("uselogging")]
        public bool UseLogging { get; set; } = false;

        /// <summary>
        /// Whether or not dates should include the time.
        /// </summary>
        [SavePropertyAs("datesincludetime")]
        public bool DatesIncludeTime { get; set; } = true;

        [SavePropertyAs("missionsonly")]
        public bool MissionsOnly { get; set; } = false;


        [SavePropertyAs("dronemods")]
        public bool Dronemods { get; set; } = false;


        /// <summary>
        /// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
        /// </summary>
        [SavePropertyAs("printtofilefirst")]
        public bool PrintToFileFirst { get; set; } = false;

        /// <summary>
        /// Omae user name.
        /// </summary>
        [SavePropertyAs("omaeusername")]
        public string OmaeUserName { get; set; } = "";

        /// <summary>
        /// Omae password (Base64 encoded).
        /// </summary>
        [SavePropertyAs("omaepassword")]
        public string OmaePassword { get; set; } = "";

        /// <summary>
        /// Omae AutoLogin.
        /// </summary>
        [SavePropertyAs("omaeautologin")]
        public bool OmaeAutoLogin { get; set; } = false;

        [SavePropertyAs("omaeenabled")]
        public bool OmaeEnabled { get; set; } = false;

        /// <summary>
        /// Main application form.
        /// </summary>
        [DisplayIgnore]
        public frmMain MainForm { get; set; }

        /// <summary>
        /// Language.
        /// </summary>
        [SavePropertyAs("language")]
        [DropDown(new []{"en-us", "jp", "de", "fr" }, DirectDisplay = new []{ "English (US)", "日本語 (JP)", "Deutsch (DE)" , "Français (FR)" })]
        public string Language { get; set; } = "en-us";

        /// <summary>
        /// Whether or not the application should start in fullscreen mode.
        /// </summary>
        [SavePropertyAs("startupfullscreen")]
        public bool StartupFullscreen { get; set; } = false;

        /// <summary>
        /// Whether or not only a single instance of the Dice Roller should be allowed.
        /// </summary>
        [SavePropertyAs("singlediceroller")]
        public bool SingleDiceRoller { get; set; } = true;

        /// <summary>
        /// CultureInfor for number localization.
        /// </summary>
        public CultureInfo CultureInfo { get; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Clipboard.
        /// </summary>
        [DisplayIgnore]
        public XmlDocument Clipboard { get; set; } = new XmlDocument();

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        [DisplayIgnore]
        public ClipboardContentType ClipboardContentType { get; set; } = new ClipboardContentType();

        

        /// <summary>
        /// Default character sheet to use when printing.
        /// </summary>
        [DropDown(" Chummer.Classes.CharacterSheetsHelper.GetListOfCharacterSheets")]
        public string DefaultCharacterSheet { get; set; } = "Shadowrun 5";

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        [SavePropertyAs("pdfapppath")]
        [IsPath(Filter = "Programs|*.exe")]
        public string PDFAppPath { get; set; } = "";

        /// <summary>
        /// How to open pdfs
        /// </summary>
        public PdfMode PdfMode { get; set; } = PdfMode.Parameter;

        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public List<SourcebookInfo> SourcebookInfo { get; set; } = new List<SourcebookInfo>();

        /// <summary>
        /// Which method of opening PDFs to use. True = file://path.pdf#page=x
        /// </summary>
        //[SavePropertyAs("openpdfsasurls")]
        //public bool OpenPDFsAsURLs { get; set; } = false;

        ///// <summary>
        ///// Which paramerters to use when opening PDFs. True = ... -p SomePage; False = ... \n \a "page = SomePage"
        ///// </summary>
        //[SavePropertyAs("openpdfsasunix")]
        //public bool OpenPDFsAsAsUnix { get; set; } = false;

        [SavePropertyAs("prefernightlybuilds")]
        public bool PreferNightlyBuilds { get; set; } = false;

        [IsPath(true)]
        [SavePropertyAs("characterrosterpath")]
        public string CharacterRosterPath { get; set; } = "";

        #endregion

        #region MRU Methods

        [DisplayIgnore]
        public List<MRUEntry> MostRecentlyUsedList { get; } = new List<MRUEntry>();

        public void MRUAdd(string entryPath)
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

            if (index == -1) //Not found
            {
                MRUEntry entry = new MRUEntry(entryPath);
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
                MRUEntry entry = MostRecentlyUsedList[index];
                MostRecentlyUsedList.RemoveAt(index);
                MostRecentlyUsedList.Insert(topOfSticky, entry);
            }

            MRUChanged?.Invoke();
            //Needs to handle
            //Item already in list
            //New item
            //Stickies
            //Full stickies
        }

        public void MruToggleSticky(MRUEntry entry)
        {
            //I'm sure this can be changed to a generalized case, but i can't see how right now
            if (entry.Sticky)
            {
                int newIndex = MostRecentlyUsedList.FindIndex(x => !x.Sticky);
                int oldIndex = MostRecentlyUsedList.IndexOf(entry);
                entry.Sticky = false;
                MostRecentlyUsedList.RemoveAt(oldIndex);
                MostRecentlyUsedList.Insert(newIndex-1, entry);
            }
            else
            {
                int newIndex = MostRecentlyUsedList.FindLastIndex(x => x.Sticky);
                int oldIndex = MostRecentlyUsedList.IndexOf(entry);
                entry.Sticky = true;
                MostRecentlyUsedList.RemoveAt(oldIndex);
                MostRecentlyUsedList.Insert(newIndex + 1, entry);

            }


            MRUChanged?.Invoke();


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

    }

    public class MRUEntry
    {
        public bool Sticky { get; set; } = false;
        public string Path { get; set; }
        public MRUEntry(string path)
        {
            Path = path;
        }
    }

    public enum PdfMode
    {
        Parameter,
        Url,
        UnixMode
    }
}