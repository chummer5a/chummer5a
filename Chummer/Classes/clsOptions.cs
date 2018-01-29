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
using iTextSharp.text.pdf;

namespace Chummer
{
    public enum ClipboardContentType
    {
        None = 0,
        Gear,
        OperatingSystem,
        Cyberware,
        Bioware,
        Armor,
        Weapon,
        Vehicle,
        Lifestyle,
    }

    public class SourcebookInfo : IDisposable
    {
        string _strCode = string.Empty;
        string _strPath = string.Empty;
        int _intOffset = 0;
        PdfReader _objPdfReader = null;

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
                if (_strPath != value)
                {
                    _strPath = value;
                    _objPdfReader?.Close();
                    _objPdfReader = null;
                }
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

        internal PdfReader CachedPdfReader
        {
            get
            {
                if (_objPdfReader == null)
                {
                    Uri uriPath = new Uri(Path);
                    if (File.Exists(uriPath.LocalPath))
                    {
                        // using the "partial" param it runs much faster and I couldnt find any downsides to it
                        _objPdfReader = new PdfReader(uriPath.LocalPath, null, true);
                    }
                }
                return _objPdfReader;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _objPdfReader?.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
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
    public static class GlobalOptions
    {
        private static readonly CultureInfo s_ObjSystemCultureInfo = CultureInfo.CurrentCulture;
        private static readonly CultureInfo s_ObjInvariantCultureInfo = CultureInfo.InvariantCulture;
        private static CultureInfo s_ObjLanguageCultureInfo = CultureInfo.CurrentCulture;

        public static Action MRUChanged { get; set; }

        private static readonly RegistryKey _objBaseChummerKey;
        public const string DefaultLanguage = "en-us";
        public const string DefaultCharacterSheetDefaultValue = "Shadowrun 5 (Rating greater 0)";

        private static bool _blnAutomaticUpdate = false;
        private static bool _blnLiveCustomData = false;
        private static bool _blnLocalisedUpdatesOnly = false;
        private static bool _blnStartupFullscreen = false;
        private static bool _blnSingleDiceRoller = true;
        private static string _strLanguage = DefaultLanguage;
        private static string _strDefaultCharacterSheet = DefaultCharacterSheetDefaultValue;
        private static bool _blnDatesIncludeTime = true;
        private static bool _blnPrintToFileFirst = false;
        private static bool _lifeModuleEnabled;
        private static bool _blnDronemods = false;
        private static bool _blnDronemodsMaximumPilot = false;
        private static bool _blnPreferNightlyUpdates = false;
        private static bool _blnLiveUpdateCleanCharacterFiles = false;

        // Omae Information.
        private static bool _omaeEnabled = false;
        private static string _strOmaeUserName = string.Empty;
        private static string _strOmaePassword = string.Empty;
        private static bool _blnOmaeAutoLogin = false;

        private static XmlDocument _objXmlClipboard = new XmlDocument();
        private static ClipboardContentType _objClipboardContentType = new ClipboardContentType();

        // PDF information.
        private static string _strPDFAppPath = string.Empty;
        private static string _strPDFParameters = string.Empty;
        private static HashSet<SourcebookInfo> _lstSourcebookInfo = null;
        private static bool _blnUseLogging = false;
        private static string _strCharacterRosterPath;

        // Custom Data Directory information.
        private static readonly List<CustomDataDirectoryInfo> _lstCustomDataDirectoryInfo = new List<CustomDataDirectoryInfo>();

        #region Constructor
        /// <summary>
        /// Load a Bool Option from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        private static bool LoadBoolFromRegistry(ref bool blnStorage, string strBoolName, string strSubKey = "")
        {
            object objRegistryResult = !string.IsNullOrWhiteSpace(strSubKey) ? _objBaseChummerKey.GetValue(strBoolName) : _objBaseChummerKey.GetValue(strBoolName);
            if (objRegistryResult != null)
            {
                if (bool.TryParse(objRegistryResult.ToString(), out bool blnTemp))
                {
                    blnStorage = blnTemp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Load an Int Option from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        private static bool LoadStringFromRegistry(ref string strStorage, string strBoolName, string strSubKey = "")
        {
            object objRegistryResult = !string.IsNullOrWhiteSpace(strSubKey) ? _objBaseChummerKey.OpenSubKey(strSubKey).GetValue(strBoolName) : _objBaseChummerKey.GetValue(strBoolName);
            if (objRegistryResult != null)
            {
                strStorage = objRegistryResult.ToString();
                return true;
            }
            return false;
        }

        static GlobalOptions()
        {
            if (Utils.IsRunningInVisualStudio)
                return;

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
                    MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                }
            }

            // Automatic Update.
            LoadBoolFromRegistry(ref _blnAutomaticUpdate, "autoupdate");

            LoadBoolFromRegistry(ref _blnLiveCustomData, "livecustomdata");

            LoadBoolFromRegistry(ref _blnLiveUpdateCleanCharacterFiles, "liveupdatecleancharacterfiles");

            LoadBoolFromRegistry(ref _lifeModuleEnabled, "lifemodule");

            LoadBoolFromRegistry(ref _omaeEnabled, "omaeenabled");

            // Whether or not the app should only download localised files in the user's selected language.
            LoadBoolFromRegistry(ref _blnLocalisedUpdatesOnly, "localisedupdatesonly");

            // Whether or not the app should use logging.
            LoadBoolFromRegistry(ref _blnUseLogging, "uselogging");

            // Whether or not dates should include the time.
            LoadBoolFromRegistry(ref _blnDatesIncludeTime, "datesincludetime");

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
            string strLanguage = _strLanguage;
            if (LoadStringFromRegistry(ref strLanguage, "language"))
            {
                switch (strLanguage)
                {
                    case "en-us2":
                        strLanguage = DefaultLanguage;
                        break;
                    case "de":
                        strLanguage = "de-de";
                        break;
                    case "fr":
                        strLanguage = "fr-fr";
                        break;
                    case "jp":
                        strLanguage = "ja-jp";
                        break;
                    case "zh":
                        strLanguage = "zh-cn";
                        break;
                }
                Language = strLanguage;
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
                for (int i = 0; i < astrCustomDataDirectoryNames.Length; ++i)
                {
                    RegistryKey objLoopKey = objCustomDataDirectoryKey.OpenSubKey(astrCustomDataDirectoryNames[i]);
                    string strPath = string.Empty;
                    object objRegistryResult = objLoopKey.GetValue("Path");
                    if (objRegistryResult != null)
                        strPath = objRegistryResult.ToString().Replace("$CHUMMER", Application.StartupPath);
                    if (!string.IsNullOrEmpty(strPath) && Directory.Exists(strPath))
                    {
                        CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo
                        {
                            Name = astrCustomDataDirectoryNames[i],
                            Path = strPath
                        };
                        objRegistryResult = objLoopKey.GetValue("Enabled");
                        if (objRegistryResult != null)
                        {
                            if (bool.TryParse(objRegistryResult.ToString(), out bool blnTemp))
                                objCustomDataDirectory.Enabled = blnTemp;
                        }
                        objRegistryResult = objLoopKey.GetValue("LoadOrder");
                        if (objRegistryResult != null && int.TryParse(objRegistryResult.ToString(), out int intLoadOrder))
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
                        CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo
                        {
                            Name = Path.GetFileName(strLoopDirectoryPath),
                            Path = strLoopDirectoryPath
                        };
                        _lstCustomDataDirectoryInfo.Add(objCustomDataDirectory);
                    }
                }
            }
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

        public static bool LiveUpdateCleanCharacterFiles
        {
            get
            {
                return _blnLiveUpdateCleanCharacterFiles;
            }
            set
            {
                _blnLiveUpdateCleanCharacterFiles = value;
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
                if (value != _strLanguage)
                {
                    _strLanguage = value;
                    try
                    {
                        s_ObjLanguageCultureInfo = CultureInfo.GetCultureInfo(value);
                    }
                    catch (CultureNotFoundException)
                    {
                        s_ObjLanguageCultureInfo = SystemCultureInfo;
                    }
                }
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
                return s_ObjLanguageCultureInfo;
            }
        }

        /// <summary>
        /// Invariant CultureInfo for saving and loading of numbers.
        /// </summary>
        public static CultureInfo InvariantCultureInfo
        {
            get
            {
                return s_ObjInvariantCultureInfo;
            }
        }

        /// <summary>
        /// CultureInfo of the user's current system.
        /// </summary>
        public static CultureInfo SystemCultureInfo
        {
            get
            {
                return s_ObjSystemCultureInfo;
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
        public static ICollection<SourcebookInfo> SourcebookInfo
        {
            get
            {
                // We need to generate _lstSourcebookInfo outside of the constructor to avoid initialization cycles
                if (_lstSourcebookInfo == null)
                {
                    _lstSourcebookInfo = new HashSet<SourcebookInfo>();
                    // Retrieve the SourcebookInfo objects.
                    foreach (XmlNode objXmlBook in XmlManager.Load("books.xml").SelectNodes("/chummer/books/book[not(hide)]"))
                    {
                        string strCode = objXmlBook["code"]?.InnerText;
                        if (!string.IsNullOrEmpty(strCode))
                        {
                            SourcebookInfo objSource = new SourcebookInfo
                            {
                                Code = strCode
                            };
                            
                            try
                            {
                                string strTemp = string.Empty;
                                if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook") && !string.IsNullOrEmpty(strTemp))
                                {
                                    string[] strParts = strTemp.Split('|');
                                    objSource.Path = strParts[0];
                                    if (strParts.Length > 1 && int.TryParse(strParts[1], out int intTmp))
                                    {
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
                }
                return _lstSourcebookInfo;
            }
        }

        /// <summary>
        /// List of CustomDataDirectoryInfo.
        /// </summary>
        public static IList<CustomDataDirectoryInfo> CustomDataDirectoryInfo
        {
            get
            {
                return _lstCustomDataDirectoryInfo;
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
        public static void AddToMRUList(string strFile, string strMRUType = "mru", bool blnDoMRUChanged = true, bool blnForceDoMRUChanged = false, int intIndex = 0)
        {
            if (string.IsNullOrEmpty(strFile))
                return;

            List<string> strFiles = ReadMRUList(strMRUType);

            // Make sure the file doesn't exist in the sticky MRU list if we're adding to base MRU list.
            if (strMRUType == "mru")
            {
                List<string> strStickyFiles = ReadMRUList("stickymru");
                if (strStickyFiles.Contains(strFile))
                {
                    if (blnForceDoMRUChanged && blnDoMRUChanged && MRUChanged != null)
                        MRUChanged.Invoke();
                    return;
                }
            }
            int i = intIndex;
            // Make sure the file does not already exist in the MRU list.
            int intOldIndex = strFiles.IndexOf(strFile);
            if (intOldIndex != -1)
            {
                if (intOldIndex == intIndex)
                {
                    if (blnForceDoMRUChanged && blnDoMRUChanged && MRUChanged != null)
                        MRUChanged.Invoke();
                    return;
                }
                strFiles.RemoveAt(intOldIndex);
                if (i > intOldIndex)
                    i = intOldIndex;
            }

            strFiles.Insert(intIndex, strFile);

            if (strFiles.Count > 10)
                strFiles.RemoveRange(10, strFiles.Count - 10);

            foreach (string strItem in strFiles)
            {
                i++;
                _objBaseChummerKey.SetValue(strMRUType + i.ToString(), strItem);
            }
            if (blnDoMRUChanged && MRUChanged != null)
                MRUChanged.Invoke();
        }

        /// <summary>
        /// Remove a file from the most recently used characters.
        /// </summary>
        /// <param name="strFile">Name of the file to remove.</param>
        public static void RemoveFromMRUList([NotNull] string strFile, string strMRUType = "mru", bool blnDoMRUChanged = true, bool blnForceDoMRUChanged = false)
        {
            List<string> strFiles = ReadMRUList(strMRUType);

            int intFileIndex = strFiles.IndexOf(strFile);
            if (intFileIndex != -1)
            {
                strFiles.RemoveAt(intFileIndex);
                for (int i = intFileIndex; i < 10; i++)
                {
                    if (i < strFiles.Count)
                        _objBaseChummerKey.SetValue(strMRUType + (i + 1).ToString(), strFiles[i]);
                    else
                        _objBaseChummerKey.DeleteValue(strMRUType + (i + 1).ToString(), false);
                }
                if (blnDoMRUChanged && MRUChanged != null)
                    MRUChanged.Invoke();
            }
            else if (blnForceDoMRUChanged && blnDoMRUChanged && MRUChanged != null)
                MRUChanged.Invoke();
        }

        /// <summary>
        /// Add a list of files to the beginning of the most recently used characters.
        /// </summary>
        /// <param name="lstFilesToAdd">Names of the files to add (files added in reverse order).</param>
        public static void AddToMRUList(IEnumerable<string> lstFilesToAdd, string strMRUType = "mru", bool blnDoMRUChanged = true)
        {
            bool blnAnyChange = false;
            List<string> strFiles = ReadMRUList(strMRUType);
            List<string> strStickyFiles = strMRUType == "mru" ? ReadMRUList("stickymru") : null;
            foreach (string strFile in lstFilesToAdd)
            {
                if (string.IsNullOrEmpty(strFile))
                    continue;
                // Make sure the file doesn't exist in the sticky MRU list if we're adding to base MRU list.
                if (strStickyFiles?.Contains(strFile) == true)
                    continue;

                blnAnyChange = true;

                // Make sure the file does not already exist in the MRU list.
                if (strFiles.Contains(strFile))
                    strFiles.Remove(strFile);

                strFiles.Insert(0, strFile);
            }

            if (strFiles.Count > 10)
                strFiles.RemoveRange(10, strFiles.Count - 10);

            if (blnAnyChange)
            {
                int i = 0;
                foreach (string strItem in strFiles)
                {
                    i++;
                    _objBaseChummerKey.SetValue(strMRUType + i.ToString(), strItem);
                }
                if (blnDoMRUChanged && MRUChanged != null)
                    MRUChanged.Invoke();
            }
        }

        /// <summary>
        /// Remove a list of files from the most recently used characters.
        /// </summary>
        /// <param name="lstFilesToRemove">Names of the files to remove.</param>
        public static void RemoveFromMRUList(IEnumerable<string> lstFilesToRemove, string strMRUType = "mru", bool blnDoMRUChanged = true)
        {
            List<string> strFiles = ReadMRUList(strMRUType);
            bool blnAnyChange = false;
            foreach (string strFile in lstFilesToRemove)
            {
                if (strFiles.Contains(strFile))
                {
                    strFiles.Remove(strFile);
                    blnAnyChange = true;
                }
            }
            if (blnAnyChange)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (i < strFiles.Count)
                        _objBaseChummerKey.SetValue(strMRUType + (i + 1).ToString(), strFiles[i]);
                    else
                        _objBaseChummerKey.DeleteValue(strMRUType + (i + 1).ToString(), false);
                }
                if (blnDoMRUChanged && MRUChanged != null)
                    MRUChanged.Invoke();
            }
        }

        /// <summary>
        /// Retrieve the list of most recently used characters.
        /// </summary>
        public static List<string> ReadMRUList(string strMRUType = "mru")
        {
            List<string> lstFiles = new List<string>(10);

            for (int i = 1; i <= 10; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue(strMRUType + i.ToString());
                if (objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if (File.Exists(strFileName) && !lstFiles.Contains(strFileName))
                        lstFiles.Add(strFileName);
                }
            }
            return lstFiles;
        }
        #endregion

    }
}
