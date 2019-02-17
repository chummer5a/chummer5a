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
using System.Globalization;
using System.IO;
 using System.Linq;
 using System.Xml;
using System.Windows.Forms;
 using Chummer.Annotations;
 using Chummer.Backend.Equipment;
 using Microsoft.Win32;

namespace Chummer
{
    public enum ClipboardContentType
    {
        None = 0,
        Gear = 1,
        Commlink = 2,
        OperatingSystem = 3,
        Cyberware = 4,
        Bioware = 5,
        Armor = 6,
        Weapon = 7,
        Vehicle = 8,
        Lifestyle = 9,
    }

    public class SourcebookInfo
    {
        string _strCode = string.Empty;
        string _strPath = string.Empty;
        int _intOffset = 0;

        #region Properties
        public string Code
        {
            get
            {
                return _strCode;
            }
            set
            {
                _strCode = value;
            }
        }

        public string Path
        {
            get
            {
                return _strPath;
            }
            set
            {
                _strPath = value;
            }
        }

        public int Offset
        {
            get
            {
                return _intOffset;
            }
            set
            {
                _intOffset = value;
            }
        }
        #endregion
    }

    public class CustomDataDirectoryInfo
    {
        private string _strName = string.Empty;
        private string _strPath = string.Empty;
        private bool _blnEnabled = false;

        #region Properties

        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        public string Path
        {
            get
            {
                return _strPath;
            }
            set
            {
                _strPath = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return _blnEnabled;
            }
            set
            {
                _blnEnabled = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Global Options. A single instance class since Options are common for all characters, reduces execution time and memory usage.
    /// </summary>
    public sealed class GlobalOptions
    {
        static readonly CultureInfo _objCultureInfo = CultureInfo.CurrentCulture;
        static readonly CultureInfo _objInvariantCultureInfo = CultureInfo.InvariantCulture;

        public static Action MRUChanged;

        private static frmMain _frmMainForm;
        private static readonly RegistryKey _objBaseChummerKey;

        private static bool _blnAutomaticUpdate = false;
        private static bool _blnLiveCustomData = false;
        private static bool _blnLocalisedUpdatesOnly = false;
        private static bool _blnStartupFullscreen = false;
        private static bool _blnSingleDiceRoller = true;
        private static string _strLanguage = "en-us";
        private static string _strDefaultCharacterSheet = "Shadowrun 5 (Rating greater 0)";
        private static bool _blnDatesIncludeTime = true;
        private static bool _blnPrintToFileFirst = false;
        private static bool _lifeModuleEnabled;
        private static bool _blnMissionsOnly = false;
        private static bool _blnDronemods = false;
        private static bool _blnDronemodsMaximumPilot = false;
        private static bool _blnPreferNightlyUpdates = false;

        // Omae Information.
        private static bool _omaeEnabled = false;
        private static string _strOmaeUserName = string.Empty;
        private static string _strOmaePassword = string.Empty;
        private static bool _blnOmaeAutoLogin = false;

        private static XmlDocument _objXmlClipboard = new XmlDocument();
        private static ClipboardContentType _objClipboardContentType = new ClipboardContentType();

        public static readonly GradeList CyberwareGrades = new GradeList();
        public static readonly GradeList BiowareGrades = new GradeList();

        // PDF information.
        private static string _strPDFAppPath = string.Empty;
        private static string _strPDFParameters = string.Empty;
        private static HashSet<SourcebookInfo> _lstSourcebookInfo = new HashSet<SourcebookInfo>();
        private static bool _blnUseLogging = false;
        private static string _strCharacterRosterPath;

        // Custom Data Directory information.
        private static List<CustomDataDirectoryInfo> _lstCustomDataDirectoryInfo = new List<CustomDataDirectoryInfo>();

        #region Constructor
        /// <summary>
        /// Load a Bool Option from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        private static void LoadBoolFromRegistry(ref bool blnStorage, string strBoolName, string strSubKey = "")
        {
            object objRegistryResult = !string.IsNullOrWhiteSpace(strSubKey) ? _objBaseChummerKey.GetValue(strBoolName) : _objBaseChummerKey.GetValue(strBoolName);
            if (objRegistryResult != null)
            {
                bool blnTemp;
                if (bool.TryParse(objRegistryResult.ToString(), out blnTemp))
                    blnStorage = blnTemp;
            }
        }

        /// <summary>
        /// Load an Int Option from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        private static void LoadStringFromRegistry(ref string strStorage, string strBoolName, string strSubKey = "")
        {
            object objRegistryResult = !string.IsNullOrWhiteSpace(strSubKey) ? _objBaseChummerKey.OpenSubKey(strSubKey).GetValue(strBoolName) : _objBaseChummerKey.GetValue(strBoolName);
            if (objRegistryResult != null)
            {
                strStorage = objRegistryResult.ToString();
            }
        }

        static GlobalOptions()
        {
            if (Utils.IsRunningInVisualStudio()) return;

            _objBaseChummerKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            if (_objBaseChummerKey == null)
                return;
            _objBaseChummerKey.CreateSubKey("Sourcebook");

            string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
            if (!Directory.Exists(settingsDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                }
            }

            // Automatic Update.
            LoadBoolFromRegistry(ref _blnAutomaticUpdate, "autoupdate");

            LoadBoolFromRegistry(ref _blnLiveCustomData, "livecustomdata");

            LoadBoolFromRegistry(ref _lifeModuleEnabled, "lifemodule");

            LoadBoolFromRegistry(ref _omaeEnabled, "omaeenabled");

            // Whether or not the app should only download localised files in the user's selected language.
            LoadBoolFromRegistry(ref _blnLocalisedUpdatesOnly, "localisedupdatesonly");

            // Whether or not the app should use logging.
            LoadBoolFromRegistry(ref _blnUseLogging, "uselogging");

            // Whether or not dates should include the time.
            LoadBoolFromRegistry(ref _blnDatesIncludeTime, "datesincludetime");

            LoadBoolFromRegistry(ref _blnMissionsOnly, "missionsonly");

            LoadBoolFromRegistry(ref _blnDronemods, "dronemods");

            LoadBoolFromRegistry(ref _blnDronemodsMaximumPilot, "dronemodsPilot");

            // Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
            LoadBoolFromRegistry(ref _blnPrintToFileFirst, "printtofilefirst");

            // Default character sheet.
            LoadStringFromRegistry(ref _strDefaultCharacterSheet, "defaultsheet");

            // Omae Settings.
            // Username.
            LoadStringFromRegistry(ref _strOmaeUserName, "omaeusername");
            // Password.
            LoadStringFromRegistry(ref _strOmaePassword, "omaepassword");
            // AutoLogin.
            LoadBoolFromRegistry(ref _blnOmaeAutoLogin, "omaeautologin");
            // Language.
            LoadStringFromRegistry(ref _strLanguage, "language");
            if (_strLanguage == "en-us2")
            {
                _strLanguage = "en-us";
            }
            // Startup in Fullscreen mode.
            LoadBoolFromRegistry(ref _blnStartupFullscreen, "startupfullscreen");
            // Single instace of the Dice Roller window.
            LoadBoolFromRegistry(ref _blnSingleDiceRoller, "singlediceroller");

            // Open PDFs as URLs. For use with Chrome, Firefox, etc.
            LoadStringFromRegistry(ref _strPDFParameters, "pdfparameters");

            // PDF application path.
            LoadStringFromRegistry(ref _strPDFAppPath, "pdfapppath");

            // Folder path to check for characters.
            LoadStringFromRegistry(ref _strCharacterRosterPath, "characterrosterpath");

            // Prefer Nightly Updates.
            LoadBoolFromRegistry(ref _blnPreferNightlyUpdates, "prefernightlybuilds");

            // Retrieve CustomDataDirectoryInfo objects from registry
            RegistryKey objCustomDataDirectoryKey = _objBaseChummerKey.OpenSubKey("CustomDataDirectory");
            if (objCustomDataDirectoryKey != null)
            {
                List<KeyValuePair<CustomDataDirectoryInfo, int>> lstUnorderedCustomDataDirectories = new List<KeyValuePair<CustomDataDirectoryInfo, int> > (objCustomDataDirectoryKey.SubKeyCount);

                string[] astrCustomDataDirectoryNames = objCustomDataDirectoryKey.GetSubKeyNames();
                int intMinLoadOrderValue = int.MaxValue;
                int intMaxLoadOrderValue = int.MinValue;
                for (int i = 0; i < astrCustomDataDirectoryNames.Count(); ++i)
                {
                    RegistryKey objLoopKey = objCustomDataDirectoryKey.OpenSubKey(astrCustomDataDirectoryNames[i]);
                    string strPath = string.Empty;
                    object objRegistryResult = objLoopKey.GetValue("Path");
                    if (objRegistryResult != null)
                        strPath = objRegistryResult.ToString();
                    if (!string.IsNullOrEmpty(strPath) && Directory.Exists(strPath))
                    {
                        CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo();
                        objCustomDataDirectory.Name = astrCustomDataDirectoryNames[i];
                        objCustomDataDirectory.Path = strPath;
                        objRegistryResult = objLoopKey.GetValue("Enabled");
                        if (objRegistryResult != null)
                        {
                            bool blnTemp;
                            if (bool.TryParse(objRegistryResult.ToString(), out blnTemp))
                                objCustomDataDirectory.Enabled = blnTemp;
                        }
                        int intLoadOrder = 0;
                        objRegistryResult = objLoopKey.GetValue("LoadOrder");
                        if (objRegistryResult != null && int.TryParse(objRegistryResult.ToString(), out intLoadOrder))
                        {
                            // First load the infos alongside their load orders into a list whose order we don't care about
                            intMaxLoadOrderValue = Math.Max(intMaxLoadOrderValue, intLoadOrder);
                            intMinLoadOrderValue = Math.Min(intMinLoadOrderValue, intLoadOrder);
                            lstUnorderedCustomDataDirectories.Add(new KeyValuePair<CustomDataDirectoryInfo, int>(objCustomDataDirectory, intLoadOrder));
                        }
                        else
                            lstUnorderedCustomDataDirectories.Add(new KeyValuePair<CustomDataDirectoryInfo, int>(objCustomDataDirectory, int.MinValue));
                    }
                }

                // Now translate the list of infos whose order we don't care about into the list where we do care about the order of infos
                for (int i = intMinLoadOrderValue; i <= intMaxLoadOrderValue; ++i)
                {
                    KeyValuePair<CustomDataDirectoryInfo, int> objLoopPair = lstUnorderedCustomDataDirectories.FirstOrDefault(x => x.Value == i);
                    if (!objLoopPair.Equals(default(KeyValuePair<CustomDataDirectoryInfo, int>)))
                        _lstCustomDataDirectoryInfo.Add(objLoopPair.Key);
                }
                foreach (KeyValuePair<CustomDataDirectoryInfo, int> objLoopPair in lstUnorderedCustomDataDirectories.Where(x => x.Value == int.MinValue))
                {
                    _lstCustomDataDirectoryInfo.Add(objLoopPair.Key);
                }
            }
            // Auto-populate the rest of the list from customdata
            string strCustomDataRootPath = Path.Combine(Application.StartupPath, "customdata");
            if (Directory.Exists(strCustomDataRootPath))
            {
                foreach (string strLoopDirectoryPath in Directory.GetDirectories(strCustomDataRootPath))
                {
                    // Only add directories for which we don't already have entries loaded from registry
                    if (!_lstCustomDataDirectoryInfo.Any(x => x.Path == strLoopDirectoryPath))
                    {
                        CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo();
                        objCustomDataDirectory.Name = Path.GetFileName(strLoopDirectoryPath);
                        objCustomDataDirectory.Path = strLoopDirectoryPath;
                        _lstCustomDataDirectoryInfo.Add(objCustomDataDirectory);
                    }
                }
            }

            // Retrieve the SourcebookInfo objects.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml");
            foreach (XmlNode objXmlBook in objXmlDocument.SelectNodes("/chummer/books/book"))
            {
                if (objXmlBook["code"] != null && objXmlBook["hide"] == null)
                {
                    SourcebookInfo objSource = new SourcebookInfo();
                    objSource.Code = objXmlBook["code"].InnerText;
                    string strTemp = string.Empty;

                    try
                    {
                        LoadStringFromRegistry(ref strTemp, objXmlBook["code"].InnerText, "Sourcebook");
                        if (!string.IsNullOrEmpty(strTemp))
                        {

                            string[] strParts = strTemp.Split('|');
                            objSource.Path = strParts[0];
                            if (strParts.Length > 1)
                            {
                                int intTmp;
                                if (int.TryParse(strParts[1], out intTmp))
                                    objSource.Offset = intTmp;
                            }
                        }
                    }
                    catch (System.Security.SecurityException)
                    {

                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                    _lstSourcebookInfo.Add(objSource);
                }
            }

            CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware);
            BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not Automatic Updates are enabled.
        /// </summary>
        public static bool AutomaticUpdate
        {
            get
            {
                return _blnAutomaticUpdate;
            }
            set
            {
                _blnAutomaticUpdate = value;
            }
        }

        /// <summary>
        /// Whether or not live updates from the customdata directory are allowed.
        /// </summary>
        public static bool LiveCustomData
        {
            get
            {
                return _blnLiveCustomData;
            }
            set
            {
                _blnLiveCustomData = value;
            }
        }

        public static bool LifeModuleEnabled
        {
            get { return _lifeModuleEnabled; }
            set { _lifeModuleEnabled = value; }
        }

        /// <summary>
        /// Whether or not the app should only download localised files in the user's selected language.
        /// </summary>
        public static bool LocalisedUpdatesOnly
        {
            get
            {
                return _blnLocalisedUpdatesOnly;
            }
            set
            {
                _blnLocalisedUpdatesOnly = value;
            }
        }

        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        public static bool UseLogging
        {
            get
            {
                return _blnUseLogging;
            }
            set
            {
                _blnUseLogging = value;
            }
        }

        /// <summary>
        /// Whether or not dates should include the time.
        /// </summary>
        public static bool DatesIncludeTime
        {
            get
            {
                return _blnDatesIncludeTime;
            }
            set
            {
                _blnDatesIncludeTime = value;
            }
        }

        public static bool MissionsOnly
        {
            get
            {
                return _blnMissionsOnly;

            }
            set
            {
                _blnMissionsOnly = value;
            }
        }

        public static bool Dronemods
        {
            get
            {
                return _blnDronemods;

            }
            set
            {
                _blnDronemods = value;
            }
        }

        public static bool DronemodsMaximumPilot
        {
            get
            {
                return _blnDronemodsMaximumPilot;
            }
            set { _blnDronemodsMaximumPilot = value; }
        }


        /// <summary>
        /// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
        /// </summary>
        public static bool PrintToFileFirst
        {
            get
            {
                return _blnPrintToFileFirst;
            }
            set
            {
                _blnPrintToFileFirst = value;
            }
        }

        /// <summary>
        /// Omae user name.
        /// </summary>
        public static string OmaeUserName
        {
            get
            {
                return _strOmaeUserName;
            }
            set
            {
                _strOmaeUserName = value;
            }
        }

        /// <summary>
        /// Omae password (Base64 encoded).
        /// </summary>
        public static string OmaePassword
        {
            get
            {
                return _strOmaePassword;
            }
            set
            {
                _strOmaePassword = value;
            }
        }

        /// <summary>
        /// Omae AutoLogin.
        /// </summary>
        public static bool OmaeAutoLogin
        {
            get
            {
                return _blnOmaeAutoLogin;
            }
            set
            {
                _blnOmaeAutoLogin = value;
            }
        }

        /// <summary>
        /// Main application form.
        /// </summary>
        public static frmMain MainForm
        {
            get
            {
                return _frmMainForm;
            }
            set
            {
                _frmMainForm = value;
            }
        }

        /// <summary>
        /// Language.
        /// </summary>
        public static string Language
        {
            get
            {
                return _strLanguage;
            }
            set
            {
                _strLanguage = value;
            }
        }

        /// <summary>
        /// Whether or not the application should start in fullscreen mode.
        /// </summary>
        public static bool StartupFullscreen
        {
            get
            {
                return _blnStartupFullscreen;
            }
            set
            {
                _blnStartupFullscreen = value;
            }
        }

        /// <summary>
        /// Whether or not only a single instance of the Dice Roller should be allowed.
        /// </summary>
        public static bool SingleDiceRoller
        {
            get
            {
                return _blnSingleDiceRoller;
            }
            set
            {
                _blnSingleDiceRoller = value;
            }
        }

        /// <summary>
        /// CultureInfo for number localization.
        /// </summary>
        public static CultureInfo CultureInfo
        {
            get
            {
                return _objCultureInfo;
            }
        }

        /// <summary>
        /// Invariant CultureInfo for saving and loading of numbers.
        /// </summary>
        public static CultureInfo InvariantCultureInfo
        {
            get
            {
                return _objInvariantCultureInfo;
            }
        }

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static XmlDocument Clipboard
        {
            get
            {
                return _objXmlClipboard;
            }
            set
            {
                _objXmlClipboard = value;
            }
        }

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public static ClipboardContentType ClipboardContentType
        {
            get
            {
                return _objClipboardContentType;
            }
            set
            {
                _objClipboardContentType = value;
            }
        }

        /// <summary>
        /// Default character sheet to use when printing.
        /// </summary>
        public static string DefaultCharacterSheet
        {
            get
            {
                return _strDefaultCharacterSheet;
            }
            set
            {
                _strDefaultCharacterSheet = value;
            }
        }

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        public static string PDFAppPath
        {
            get
            {
                return _strPDFAppPath;
            }
            set
            {
                _strPDFAppPath = value;
            }
        }

        public static string PDFParameters
        {
            get { return _strPDFParameters;}
            set { _strPDFParameters = value; }
        }
        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public static HashSet<SourcebookInfo> SourcebookInfo
        {
            get
            {
                return _lstSourcebookInfo;
            }
            set
            {
                _lstSourcebookInfo = value;
            }
        }

        /// <summary>
        /// List of CustomDataDirectoryInfo.
        /// </summary>
        public static List<CustomDataDirectoryInfo> CustomDataDirectoryInfo
        {
            get
            {
                return _lstCustomDataDirectoryInfo;
            }
            set
            {
                _lstCustomDataDirectoryInfo = value;
            }
        }

        public static bool OmaeEnabled
        {
            get { return _omaeEnabled; }
            set { _omaeEnabled = value; }
        }

        public static bool PreferNightlyBuilds
        {
            get
            {
                return _blnPreferNightlyUpdates;
            }
            set
            {
                _blnPreferNightlyUpdates = value;
            }
        }

        public static string CharacterRosterPath
        {
            get
            {
                return _strCharacterRosterPath;
            }
            set
            {
                _strCharacterRosterPath = value;
            }
        }

        public static string PDFArguments { get; internal set; }
        #endregion

        #region MRU Methods
        /// <summary>
        /// Add a file to the most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to add.</param>
        public static void AddToMRUList(string strFile, string strMRUType = "mru")
        {
            List<string> strFiles = ReadMRUList(strMRUType);

            // Make sure the file doesn't exist in the sticky MRU list if we're adding to base MRU list.
            if (strMRUType == "mru")
            {
                List<string> strStickyFiles = ReadMRUList("stickymru");
                if (strStickyFiles.Contains(strFile))
                    return;
            }
            // Make sure the file does not already exist in the MRU list.
            if (strFiles.Contains(strFile))
                strFiles.Remove(strFile);

            strFiles.Insert(0, strFile);

            if (strFiles.Count > 10)
                strFiles.RemoveRange(10, strFiles.Count - 10);

            int i = 0;
            foreach (string strItem in strFiles)
            {
                i++;
                _objBaseChummerKey.SetValue(strMRUType + i.ToString(), strItem);
            }
            MRUChanged?.Invoke();
        }

        /// <summary>
        /// Remove a file from the most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to remove.</param>
        public static void RemoveFromMRUList([NotNull] string strFile, string strMRUType = "mru")
        {
            List<string> strFiles = ReadMRUList(strMRUType);

            if (strFile.Contains(strFile))
            {
                strFiles.Remove(strFile);
            }
            for (int i = 0; i < 10; i++)
            {
                if (_objBaseChummerKey.GetValue(strMRUType + i.ToString()) != null)
                    _objBaseChummerKey.DeleteValue(strMRUType + i.ToString());
            }
            for (int i = 0; i < strFiles.Count; i++)
            {
                _objBaseChummerKey.SetValue(strMRUType + (i + 1).ToString(), strFiles[i]);
            }
            MRUChanged?.Invoke();
        }

        /// <summary>
        /// Retrieve the list of most recently used characters.
        /// </summary>
        public static List<string> ReadMRUList(string strMRUType = "mru")
        {
            List<string> lstFiles = new List<string>();

            for (int i = 1; i <= 10; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue(strMRUType + i.ToString());
                if (objLoopValue != null)
                {
                    lstFiles.Add(objLoopValue.ToString());
                }
            }
            lstFiles = lstFiles.Distinct().ToList();
            return lstFiles;
        }
        #endregion

    }
}
