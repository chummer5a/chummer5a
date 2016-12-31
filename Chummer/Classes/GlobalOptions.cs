using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Attributes.SaveAttributes;
using Chummer.Backend.Equipment;
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
            if (Utils.IsRunningInVisualStudio()) return;

            string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
            if (!Directory.Exists(settingsDirectoryPath))
                Directory.CreateDirectory(settingsDirectoryPath);

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
                        RegistryKey bookKey = Registry.CurrentConfig.CreateSubKey("Software\\Chummer5\\Books");
                        foreach (RegistryKey specificBook in bookKey.GetSubKeyNames()
                            .Select(x => bookKey.OpenSubKey(x)))
                        {
                            string bookCode = specificBook.GetValue("code").ToString();
                            SourcebookInfo info = _instance.SourcebookInfo.FirstOrDefault(x => x.Code == bookCode);
                            if (info != null)
                                loader.Load(ref info, specificBook);
                        }
                    }
                } catch {}
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

        /// <summary>
        /// Main application form.
        /// </summary>
        public frmMain MainForm { get; set; }

        /// <summary>
        /// Language.
        /// </summary>
        [SavePropertyAs("language")]
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
        public XmlDocument Clipboard { get; set; } = new XmlDocument();

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public ClipboardContentType ClipboardContentType { get; set; } = new ClipboardContentType();

        /// <summary>
        /// Default character sheet to use when printing.
        /// </summary>
        public string DefaultCharacterSheet { get; set; } = "Shadowrun 5";

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        [SavePropertyAs("pdfapppath")]
        public string PDFAppPath { get; set; } = "";

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        public string URLAppPath { get; set; } = "";

        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public List<SourcebookInfo> SourcebookInfo { get; set; } = new List<SourcebookInfo>();

        /// <summary>
        /// Which method of opening PDFs to use. True = file://path.pdf#page=x
        /// </summary>
        [SavePropertyAs("openpdfsasurls")]
        public bool OpenPDFsAsURLs { get; set; } = false;

        /// <summary>
        /// Which paramerters to use when opening PDFs. True = ... -p SomePage; False = ... \n \a "page = SomePage"
        /// </summary>
        [SavePropertyAs("openpdfsasunix")]
        public bool OpenPDFsAsAsUnix { get; set; } = false;

        [SavePropertyAs("omaeenabled")]
        public bool OmaeEnabled { get; set; } = false;

        [SavePropertyAs("prefernightlybuilds")]
        public bool PreferNightlyBuilds { get; set; } = false;

        [SavePropertyAs("characterrosterpath")]
        public string CharacterRosterPath { get; set; }

        #endregion

        #region MRU Methods
        /// <summary>
        /// Add a file to the most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to add.</param>
        public void AddToMRUList(string strFile)
        {
            List<string> strFiles = ReadMRUList();

            // Make sure the file does not already exist in the MRU list.
            if (strFiles.Contains(strFile))
                strFiles.Remove(strFile);

            // Make sure the file doesn't exist in the sticky MRU list.
            List<string> strStickyFiles = ReadStickyMRUList();
            if (strStickyFiles.Contains(strFile))
                return;

            strFiles.Insert(0, strFile);

            if (strFiles.Count > 10)
                strFiles.RemoveRange(10, strFiles.Count - 10);

            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            int i = 0;
            foreach (string strItem in strFiles)
            {
                i++;
                objRegistry.SetValue("mru" + i.ToString(), strItem);
            }
            MRUChanged();
        }

        /// <summary>
        /// Remove a file from the most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to remove.</param>
        public void RemoveFromMRUList(string strFile)
        {
            List<string> strFiles = ReadMRUList();

            foreach (string strItem in strFiles)
            {
                if (strItem == strFile)
                {
                    strFiles.Remove(strItem);
                    break;
                }
            }

            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            int i = 0;
            foreach (string strItem in strFiles)
            {
                i++;
                objRegistry.SetValue("mru" + i.ToString(), strItem);
            }
            if (strFiles.Count < 10)
            {
                for (i = strFiles.Count + 1; i <= 10; i++)
                {
                    try
                    {
                        objRegistry.DeleteValue("mru" + i.ToString());
                    }
                    catch
                    {
                    }
                }
            }
            MRUChanged();
        }

        /// <summary>
        /// Retrieve the list of most recently used characters.
        /// </summary>
        public List<string> ReadMRUList()
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
        /// Add a file to the sticky most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to add.</param>
        public void AddToStickyMRUList(string strFile)
        {
            List<string> strFiles = ReadStickyMRUList();

            // Make sure the file does not already exist in the MRU list.
            if (strFiles.Contains(strFile))
                strFiles.Remove(strFile);

            strFiles.Insert(0, strFile);

            if (strFiles.Count > 10)
                strFiles.RemoveRange(10, strFiles.Count - 10);

            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            int i = 0;
            foreach (string strItem in strFiles)
            {
                i++;
                objRegistry.SetValue("stickymru" + i.ToString(), strItem);
            }
            MRUChanged();
        }

        /// <summary>
        /// Remove a file from the sticky most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to remove.</param>
        public void RemoveFromStickyMRUList(string strFile)
        {
            List<string> strFiles = ReadStickyMRUList();

            foreach (string strItem in strFiles)
            {
                if (strItem == strFile)
                {
                    strFiles.Remove(strItem);
                    break;
                }
            }

            RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            int i = 0;
            foreach (string strItem in strFiles)
            {
                i++;
                objRegistry.SetValue("stickymru" + i.ToString(), strItem);
            }
            if (strFiles.Count < 10)
            {
                for (i = strFiles.Count + 1; i <= 10; i++)
                {
                    try
                    {
                        objRegistry.DeleteValue("stickymru" + i.ToString());
                    }
                    catch
                    {
                    }
                }
            }
            MRUChanged();
        }

        /// <summary>
        /// Retrieve the list of sticky most recently used characters.
        /// </summary>
        public List<string> ReadStickyMRUList()
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
}