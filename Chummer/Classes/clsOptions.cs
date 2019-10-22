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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using Microsoft.Win32;
using iTextSharp.text.pdf;
using MersenneTwister;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;

namespace Chummer
{
    public enum ClipboardContentType
    {
        None = 0,
        Armor,
        ArmorMod,
        Cyberware,
        Gear,
        Lifestyle,
        Vehicle,
        Weapon,
        WeaponAccessory
    }

    public enum UseAILogging
    {
        OnlyLocal = 0,
        OnlyMetric,
        Crashes,
        NotSet,
        Info,
        Trace
    }

    public sealed class SourcebookInfo : IDisposable
    {
        string _strPath = string.Empty;
        PdfReader _objPdfReader;

        #region Properties
        public string Code { get; set; } = string.Empty;

        public string Path
        {
            get => _strPath;
            set
            {
                if(_strPath != value)
                {
                    _strPath = value;
                    _objPdfReader?.Close();
                    _objPdfReader = null;
                }
            }
        }

        public int Offset { get; set; }

        internal PdfReader CachedPdfReader
        {
            get
            {
                if(_objPdfReader == null)
                {
                    Uri uriPath = new Uri(Path);
                    if(File.Exists(uriPath.LocalPath))
                    {
                        // using the "partial" param it runs much faster and I couldnt find any downsides to it
                        _objPdfReader = new PdfReader(uriPath.LocalPath, null, true);
                    }
                }
                return _objPdfReader;
            }
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
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

    /// <summary>
    /// Global Options. A single instance class since Options are common for all characters, reduces execution time and memory usage.
    /// </summary>
    public static class GlobalOptions
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static CultureInfo s_ObjLanguageCultureInfo = CultureInfo.GetCultureInfo(DefaultLanguage);

        public static string ErrorMessage { get; } = string.Empty;
        public static event TextEventHandler MRUChanged;
        public static event PropertyChangedEventHandler ClipboardChanged;

        public const int MaxMruSize = 10;
        private static readonly MostRecentlyUsedCollection<string> _lstMostRecentlyUsedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);
        private static readonly MostRecentlyUsedCollection<string> _lstFavoritedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);

        private static readonly RegistryKey _objBaseChummerKey;
        public const string DefaultLanguage = "en-us";
        public const string DefaultCharacterSheetDefaultValue = "Shadowrun 5 (Skills grouped by Rating greater 0)";
        public const string DefaultBuildMethodDefaultValue = "Priority";
        public const string DefaultGameplayOptionDefaultValue = "Standard";

        private static bool _blnAutomaticUpdate;
        private static bool _blnLiveCustomData;
        private static bool _blnStartupFullscreen;
        private static bool _blnSingleDiceRoller = true;
        private static string _strLanguage = DefaultLanguage;
        private static string _strDefaultCharacterSheet = DefaultCharacterSheetDefaultValue;
        private static bool _blnDatesIncludeTime = true;
        private static bool _blnPrintToFileFirst;
        private static int _intEmulatedBrowserVersion = 8;
        private static bool _lifeModuleEnabled;
        private static bool _blnDronemods;
        private static bool _blnDronemodsMaximumPilot;
        private static bool _blnPreferNightlyUpdates;
        private static bool _blnLiveUpdateCleanCharacterFiles;
        private static bool _blnHideCharacterRoster;
        private static bool _blnCreateBackupOnCareer;
        private static bool _blnPluginsEnabled;
        private static bool _blnAllowEasterEggs;
        private static bool _blnHideCharts;
        private static bool _blnCustomDateTimeFormats;
        private static string _strCustomDateFormat;
        private static string _strCustomTimeFormat;
        private static string _strDefaultBuildMethod = DefaultBuildMethodDefaultValue;
        private static string _strDefaultGameplayOption = DefaultGameplayOptionDefaultValue;

        public static ThreadSafeRandom RandomGenerator { get; } = new ThreadSafeRandom(DsfmtRandom.Create(DsfmtEdition.OptGen_216091));

        // Omae Information.
        private static bool _omaeEnabled;
        private static string _strOmaeUserName = string.Empty;
        private static string _strOmaePassword = string.Empty;
        private static bool _blnOmaeAutoLogin;

        // Plugins information
        private static Dictionary<string, bool> _pluginsEnabledDic = new Dictionary<string, bool>();
        public static Dictionary<string, bool> PluginsEnabledDic { get { return _pluginsEnabledDic; } }

        // PDF information.
        private static string _strPDFAppPath = string.Empty;
        private static string _strPDFParameters = string.Empty;
        private static HashSet<SourcebookInfo> _lstSourcebookInfo;
        private static bool _blnUseLogging;
        private static UseAILogging _enumUseLoggingApplicationInsights;
        private static string _strCharacterRosterPath;

        // Custom Data Directory information.
        private static readonly List<CustomDataDirectoryInfo> _lstCustomDataDirectoryInfo = new List<CustomDataDirectoryInfo>();

        #region Constructor
        /// <summary>
        /// Load a Bool Option from the Registry.
        /// </summary>
        public static bool LoadBoolFromRegistry(ref bool blnStorage, string strBoolName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = _objBaseChummerKey;
            if(!string.IsNullOrWhiteSpace(strSubKey))
                objKey = objKey.OpenSubKey(strSubKey);
            if(objKey != null)
            {
                object objRegistryResult = objKey.GetValue(strBoolName);
                if(objRegistryResult != null)
                {
                    if(bool.TryParse(objRegistryResult.ToString(), out bool blnTemp))
                        blnStorage = blnTemp;
                    if(!string.IsNullOrWhiteSpace(strSubKey))
                        objKey.Close();
                    if(blnDeleteAfterFetch)
                        objKey.DeleteValue(strBoolName);
                    return true;
                }

                if(!string.IsNullOrWhiteSpace(strSubKey))
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load an Int Option from the Registry.
        /// </summary>
        public static bool LoadInt32FromRegistry(ref int intStorage, string strIntName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = _objBaseChummerKey;
            if(!string.IsNullOrWhiteSpace(strSubKey))
                objKey = objKey.OpenSubKey(strSubKey);
            if(objKey != null)
            {
                object objRegistryResult = objKey.GetValue(strIntName);
                if(objRegistryResult != null)
                {
                    if(int.TryParse(objRegistryResult.ToString(), out int intTemp))
                        intStorage = intTemp;
                    if(blnDeleteAfterFetch)
                        objKey.DeleteValue(strIntName);
                    if(!string.IsNullOrWhiteSpace(strSubKey))
                        objKey.Close();
                    return true;
                }

                if(!string.IsNullOrWhiteSpace(strSubKey))
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load a Decimal Option from the Registry.
        /// </summary>
        public static bool LoadDecFromRegistry(ref decimal decStorage, string strDecName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = _objBaseChummerKey;
            if(!string.IsNullOrWhiteSpace(strSubKey))
                objKey = objKey.OpenSubKey(strSubKey);
            if(objKey != null)
            {
                object objRegistryResult = objKey.GetValue(strDecName);
                if(objRegistryResult != null)
                {
                    if(decimal.TryParse(objRegistryResult.ToString(), NumberStyles.Any, InvariantCultureInfo, out decimal decTemp))
                        decStorage = decTemp;
                    if(blnDeleteAfterFetch)
                        objKey.DeleteValue(strDecName);
                    if(!string.IsNullOrWhiteSpace(strSubKey))
                        objKey.Close();
                    return true;
                }

                if(!string.IsNullOrWhiteSpace(strSubKey))
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load an String Option from the Registry.
        /// </summary>
        public static bool LoadStringFromRegistry(ref string strStorage, string strStringName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = _objBaseChummerKey;
            if(!string.IsNullOrWhiteSpace(strSubKey))
                objKey = objKey.OpenSubKey(strSubKey);
            if(objKey != null)
            {
                object objRegistryResult = objKey.GetValue(strStringName);
                if(objRegistryResult != null)
                {
                    strStorage = objRegistryResult.ToString();
                    if(blnDeleteAfterFetch)
                        objKey.DeleteValue(strStringName);
                    if(!string.IsNullOrWhiteSpace(strSubKey))
                        objKey.Close();
                    return true;
                }

                if(!string.IsNullOrWhiteSpace(strSubKey))
                    objKey.Close();
            }

            return false;
        }

        static GlobalOptions()
        {
            if(Utils.IsDesignerMode)
                return;

            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
            if(!Directory.Exists(settingsDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }
                catch(UnauthorizedAccessException ex)
                {
                    string strMessage = LanguageManager.GetString("Message_Insufficient_Permissions_Warning", Language, false);
                    if(string.IsNullOrEmpty(strMessage))
                        strMessage = ex.ToString();
                    ErrorMessage += strMessage;
                }
                catch(Exception ex)
                {
                    ErrorMessage += ex.ToString();
                }
            }

            try
            {
                _objBaseChummerKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            }
            catch(Exception ex)
            {
                if(!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage += Environment.NewLine + Environment.NewLine;
                ErrorMessage += ex.ToString();
            }
            if(_objBaseChummerKey == null)
                return;
            _objBaseChummerKey.CreateSubKey("Sourcebook");

            // Automatic Update.
            LoadBoolFromRegistry(ref _blnAutomaticUpdate, "autoupdate");

            LoadBoolFromRegistry(ref _blnLiveCustomData, "livecustomdata");

            LoadBoolFromRegistry(ref _blnLiveUpdateCleanCharacterFiles, "liveupdatecleancharacterfiles");

            LoadBoolFromRegistry(ref _lifeModuleEnabled, "lifemodule");

            LoadBoolFromRegistry(ref _omaeEnabled, "omaeenabled");

            // Whether or not the app should use logging.
            LoadBoolFromRegistry(ref _blnUseLogging, "uselogging");

            //Should the App "Phone home"
            {
                try
                {
                    string useAI = "NotSet";
                    LoadStringFromRegistry(ref useAI, "useloggingApplicationInsights");
                    switch (useAI)
                    {
                        //legacy convert of checkbox bool
                        case "False":
                            _enumUseLoggingApplicationInsights = UseAILogging.NotSet;
                            break;
                        //legacy convert of checkbox bool
                        case "True":
                            _enumUseLoggingApplicationInsights = UseAILogging.Info;
                            break;
                        case "Yes":
                            _enumUseLoggingApplicationInsights = UseAILogging.Info;
                            break;
                        default:
                            _enumUseLoggingApplicationInsights = (UseAILogging) Enum.Parse(typeof(UseAILogging), useAI);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    _enumUseLoggingApplicationInsights = UseAILogging.NotSet;
                }
                
            }

            // Whether or not dates should include the time.
            LoadBoolFromRegistry(ref _blnDatesIncludeTime, "datesincludetime");

            LoadBoolFromRegistry(ref _blnDronemods, "dronemods");

            LoadBoolFromRegistry(ref _blnDronemodsMaximumPilot, "dronemodsPilot");

            LoadBoolFromRegistry(ref _blnHideCharacterRoster, "hidecharacterroster");

            LoadBoolFromRegistry(ref _blnCreateBackupOnCareer, "createbackuponcareer");

            // Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
            LoadBoolFromRegistry(ref _blnPrintToFileFirst, "printtofilefirst");

            // Which version of the Internet Explorer's rendering engine will be emulated for rendering the character view.
            LoadInt32FromRegistry(ref _intEmulatedBrowserVersion, "emulatedbrowserversion");

            // Default character sheet.
            LoadStringFromRegistry(ref _strDefaultCharacterSheet, "defaultsheet");
            if(_strDefaultCharacterSheet == "Shadowrun (Rating greater 0)")
                _strDefaultCharacterSheet = DefaultCharacterSheetDefaultValue;

            LoadStringFromRegistry(ref _strDefaultBuildMethod, "defaultbuildmethod");

            LoadStringFromRegistry(ref _strDefaultGameplayOption, "defaultgameplayoption");

            LoadBoolFromRegistry(ref _blnAllowEasterEggs, "alloweastereggs");

            // Omae Settings.
            // Username.
            LoadStringFromRegistry(ref _strOmaeUserName, "omaeusername");
            // Password.
            LoadStringFromRegistry(ref _strOmaePassword, "omaepassword");
            // AutoLogin.
            LoadBoolFromRegistry(ref _blnOmaeAutoLogin, "omaeautologin");
            // Language.
            string strLanguage = _strLanguage;
            if(LoadStringFromRegistry(ref strLanguage, "language"))
            {
                switch(strLanguage)
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

            // Which Plugins are enabled.
            LoadBoolFromRegistry(ref _blnPluginsEnabled, "pluginsenabled");

            try
            {
                string jsonstring = "";
                LoadStringFromRegistry(ref jsonstring, "plugins");
                if(!String.IsNullOrEmpty(jsonstring))
                    _pluginsEnabledDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(jsonstring);

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
#if DEBUG
                throw;
//#else
                string msg = "Error while loading PluginOptions from registry: " + Environment.NewLine;
                msg += e.Message;
                Program.MainForm.ShowMessageBox(msg);
#endif
            }
            
            // Prefer Nightly Updates.
            LoadBoolFromRegistry(ref _blnPreferNightlyUpdates, "prefernightlybuilds");

            // Prefer Nightly Updates.
            LoadBoolFromRegistry(ref _blnHideCharts, "hidecharts");

            LoadBoolFromRegistry(ref _blnCustomDateTimeFormats, "customdatetimeformats");
            LoadStringFromRegistry(ref _strCustomDateFormat, "customdateformat");
            LoadStringFromRegistry(ref _strCustomTimeFormat, "customtimeformat");

            RebuildCustomDataDirectoryInfoList();

            for(int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue("stickymru" + i.ToString(InvariantCultureInfo));
                if(objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if(File.Exists(strFileName) && !_lstFavoritedCharacters.Contains(strFileName))
                        _lstFavoritedCharacters.Add(strFileName);
                }
            }
            _lstFavoritedCharacters.CollectionChanged += LstFavoritedCharactersOnCollectionChanged;

            for(int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = _objBaseChummerKey.GetValue("mru" + i.ToString(InvariantCultureInfo));
                if(objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if(File.Exists(strFileName) && !_lstMostRecentlyUsedCharacters.Contains(strFileName))
                        _lstMostRecentlyUsedCharacters.Add(strFileName);
                }
            }
            _lstMostRecentlyUsedCharacters.CollectionChanged += LstMostRecentlyUsedCharactersOnCollectionChanged;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not to create backups of characters before moving them to career mode. If true, a separate savefile is created before marking the current character as created.
        /// </summary>
        public static bool CreateBackupOnCareer
        {
            get => _blnCreateBackupOnCareer;
            set => _blnCreateBackupOnCareer = value;
        }


        /// <summary>
        /// Should the Plugins-Directory be loaded and the tabPlugins be shown?
        /// </summary>
        public static bool PluginsEnabled
        {
            get => _blnPluginsEnabled;
            set => _blnPluginsEnabled = value;
        }

        /// <summary>
        /// Should Chummer present Easter Eggs to the user?
        /// </summary>
        public static bool AllowEasterEggs
        {
            get => _blnAllowEasterEggs;
            set => _blnAllowEasterEggs = value;
        }

        /// <summary>
        /// Whether or not the Character Roster should be shown. If true, prevents the roster from being removed or hidden. 
        /// </summary>
        public static bool HideCharacterRoster
        {
            get => _blnHideCharacterRoster;
            set => _blnHideCharacterRoster = value;
        }

        /// <summary>
        /// Whether or not Automatic Updates are enabled.
        /// </summary>
        public static bool AutomaticUpdate
        {
            get => _blnAutomaticUpdate;
            set => _blnAutomaticUpdate = value;
        }

        /// <summary>
        /// Whether or not live updates from the customdata directory are allowed.
        /// </summary>
        public static bool LiveCustomData
        {
            get => _blnLiveCustomData;
            set => _blnLiveCustomData = value;
        }

        public static bool LiveUpdateCleanCharacterFiles
        {
            get => _blnLiveUpdateCleanCharacterFiles;
            set => _blnLiveUpdateCleanCharacterFiles = value;
        }

        public static bool LifeModuleEnabled
        {
            get => _lifeModuleEnabled;
            set => _lifeModuleEnabled = value;
        }

        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        public static bool UseLogging
        {
            get => _blnUseLogging;
            set
            {
                if(_blnUseLogging != value)
                {
                    _blnUseLogging = value;
                    // Sets up logging if the option is changed during runtime
                    if (value)
                        NLog.LogManager.EnableLogging();
                    else
                        NLog.LogManager.DisableLogging();
                    
                }
            }
        }

        

        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        public static UseAILogging UseLoggingApplicationInsights
        {
            get => _enumUseLoggingApplicationInsights;
            set
            {
                _enumUseLoggingApplicationInsights = value;
                // Sets up logging if the option is changed during runtime
                TelemetryConfiguration.Active.DisableTelemetry = _enumUseLoggingApplicationInsights > UseAILogging.OnlyLocal;
            }
        }
        /// <summary>
        /// Whether or not dates should include the time.
        /// </summary>
        public static bool DatesIncludeTime
        {
            get => _blnDatesIncludeTime;
            set => _blnDatesIncludeTime = value;
        }

        public static bool Dronemods
        {
            get => _blnDronemods;
            set => _blnDronemods = value;
        }

        public static bool DronemodsMaximumPilot
        {
            get => _blnDronemodsMaximumPilot;
            set => _blnDronemodsMaximumPilot = value;
        }


        /// <summary>
        /// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
        /// </summary>
        public static bool PrintToFileFirst
        {
            get => _blnPrintToFileFirst;
            set => _blnPrintToFileFirst = value;
        }

        /// <summary>
        /// Which version of the Internet Explorer's rendering engine will be emulated for rendering the character view. Defaults to 8
        /// </summary>
        public static int EmulatedBrowserVersion
        {
            get => _intEmulatedBrowserVersion;
            set => _intEmulatedBrowserVersion = value;
        }

        /// <summary>
        /// Omae user name.
        /// </summary>
        public static string OmaeUserName
        {
            get => _strOmaeUserName;
            set => _strOmaeUserName = value;
        }

        /// <summary>
        /// Omae password (Base64 encoded).
        /// </summary>
        public static string OmaePassword
        {
            get => _strOmaePassword;
            set => _strOmaePassword = value;
        }

        /// <summary>
        /// Omae AutoLogin.
        /// </summary>
        public static bool OmaeAutoLogin
        {
            get => _blnOmaeAutoLogin;
            set => _blnOmaeAutoLogin = value;
        }

        /// <summary>
        /// Language.
        /// </summary>
        public static string Language
        {
            get => _strLanguage;
            set
            {
                if(value != _strLanguage)
                {
                    _strLanguage = value;
                    try
                    {
                        s_ObjLanguageCultureInfo = CultureInfo.GetCultureInfo(value);
                    }
                    catch(CultureNotFoundException)
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
            get => _blnStartupFullscreen;
            set => _blnStartupFullscreen = value;
        }

        /// <summary>
        /// Whether or not only a single instance of the Dice Roller should be allowed.
        /// </summary>
        public static bool SingleDiceRoller
        {
            get => _blnSingleDiceRoller;
            set => _blnSingleDiceRoller = value;
        }

        /// <summary>
        /// CultureInfo for number localization.
        /// </summary>
        public static CultureInfo CultureInfo => s_ObjLanguageCultureInfo;

        /// <summary>
        /// Invariant CultureInfo for saving and loading of numbers.
        /// </summary>
        public static CultureInfo InvariantCultureInfo { get; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// CultureInfo of the user's current system.
        /// </summary>
        public static CultureInfo SystemCultureInfo { get; } = CultureInfo.CurrentCulture;

        private static XmlDocument _xmlClipboard = new XmlDocument();

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static XmlDocument Clipboard
        {
            get => _xmlClipboard;
            set
            {
                if(_xmlClipboard != value)
                {
                    _xmlClipboard = value;
                    ClipboardChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)));
                }
            }
        }

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public static ClipboardContentType ClipboardContentType { get; set; }

        /// <summary>
        /// Default character sheet to use when printing.
        /// </summary>
        public static string DefaultCharacterSheet
        {
            get => _strDefaultCharacterSheet;
            set => _strDefaultCharacterSheet = value;
        }

        /// <summary>
        /// Default build method to select when creating a new character
        /// </summary>
        public static string DefaultBuildMethod
        {
            get => _strDefaultBuildMethod;
            set => _strDefaultBuildMethod = value;
        }

        /// <summary>
        /// Default gameplay option to select when creating a new character
        /// </summary>
        public static string DefaultGameplayOption
        {
            get => _strDefaultGameplayOption;
            set => _strDefaultGameplayOption = value;
        }

        public static RegistryKey ChummerRegistryKey => _objBaseChummerKey;

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        public static string PDFAppPath
        {
            get => _strPDFAppPath;
            set => _strPDFAppPath = value;
        }

        public static string PDFParameters
        {
            get => _strPDFParameters;
            set => _strPDFParameters = value;
        }

        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public static ICollection<SourcebookInfo> SourcebookInfo
        {
            get
            {
                // We need to generate _lstSourcebookInfo outside of the constructor to avoid initialization cycles
                if(_lstSourcebookInfo == null)
                {
                    _lstSourcebookInfo = new HashSet<SourcebookInfo>();
                    // Retrieve the SourcebookInfo objects.
                    using(XmlNodeList xmlBookList = XmlManager.Load("books.xml", new Dictionary<string, bool>()).SelectNodes("/chummer/books/book[not(hide)]"))
                        if(xmlBookList != null)
                            foreach(XmlNode xmlBook in xmlBookList)
                            {
                                string strCode = xmlBook["code"]?.InnerText;
                                if(!string.IsNullOrEmpty(strCode))
                                {
                                    SourcebookInfo objSource = new SourcebookInfo
                                    {
                                        Code = strCode
                                    };

                                    try
                                    {
                                        string strTemp = string.Empty;
                                        if(LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook") && !string.IsNullOrEmpty(strTemp))
                                        {
                                            string[] strParts = strTemp.Split('|');
                                            objSource.Path = strParts[0];
                                            if(strParts.Length > 1 && int.TryParse(strParts[1], out int intTmp))
                                            {
                                                objSource.Offset = intTmp;
                                            }
                                        }
                                    }
                                    catch(System.Security.SecurityException)
                                    {

                                    }
                                    catch(UnauthorizedAccessException)
                                    {

                                    }
                                    _lstSourcebookInfo.Add(objSource);
                                }
                            }
                }
                return _lstSourcebookInfo;
            }
        }

        public static void RebuildCustomDataDirectoryInfoList()
        {
            _lstCustomDataDirectoryInfo.Clear();

            // Retrieve CustomDataDirectoryInfo objects from registry
            RegistryKey objCustomDataDirectoryKey = _objBaseChummerKey.OpenSubKey("CustomDataDirectory");
            if(objCustomDataDirectoryKey != null)
            {
                List<KeyValuePair<CustomDataDirectoryInfo, int>> lstUnorderedCustomDataDirectories = new List<KeyValuePair<CustomDataDirectoryInfo, int>>(objCustomDataDirectoryKey.SubKeyCount);

                string[] astrCustomDataDirectoryNames = objCustomDataDirectoryKey.GetSubKeyNames();
                int intMinLoadOrderValue = int.MaxValue;
                int intMaxLoadOrderValue = int.MinValue;
                for(int i = 0; i < astrCustomDataDirectoryNames.Length; ++i)
                {
                    RegistryKey objLoopKey = objCustomDataDirectoryKey.OpenSubKey(astrCustomDataDirectoryNames[i]);
                    if(objLoopKey != null)
                    {
                        string strPath = string.Empty;
                        object objRegistryResult = objLoopKey.GetValue("Path");
                        if(objRegistryResult != null)
                            strPath = objRegistryResult.ToString().Replace("$CHUMMER", Utils.GetStartupPath);
                        if(!string.IsNullOrEmpty(strPath) && Directory.Exists(strPath))
                        {
                            CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo
                            {
                                Name = astrCustomDataDirectoryNames[i],
                                Path = strPath
                            };
                            objRegistryResult = objLoopKey.GetValue("Enabled");
                            if(objRegistryResult != null)
                            {
                                if(bool.TryParse(objRegistryResult.ToString(), out bool blnTemp))
                                    objCustomDataDirectory.Enabled = blnTemp;
                            }

                            objRegistryResult = objLoopKey.GetValue("LoadOrder");
                            if(objRegistryResult != null && int.TryParse(objRegistryResult.ToString(), out int intLoadOrder))
                            {
                                // First load the infos alongside their load orders into a list whose order we don't care about
                                intMaxLoadOrderValue = Math.Max(intMaxLoadOrderValue, intLoadOrder);
                                intMinLoadOrderValue = Math.Min(intMinLoadOrderValue, intLoadOrder);
                                lstUnorderedCustomDataDirectories.Add(new KeyValuePair<CustomDataDirectoryInfo, int>(objCustomDataDirectory, intLoadOrder));
                            }
                            else
                                lstUnorderedCustomDataDirectories.Add(new KeyValuePair<CustomDataDirectoryInfo, int>(objCustomDataDirectory, int.MinValue));
                        }
                        objLoopKey.Close();
                    }
                }

                // Now translate the list of infos whose order we don't care about into the list where we do care about the order of infos
                for(int i = intMinLoadOrderValue; i <= intMaxLoadOrderValue; ++i)
                {
                    KeyValuePair<CustomDataDirectoryInfo, int> objLoopPair = lstUnorderedCustomDataDirectories.FirstOrDefault(x => x.Value == i);
                    if(!objLoopPair.Equals(default(KeyValuePair<CustomDataDirectoryInfo, int>)))
                        _lstCustomDataDirectoryInfo.Add(objLoopPair.Key);
                }
                foreach(KeyValuePair<CustomDataDirectoryInfo, int> objLoopPair in lstUnorderedCustomDataDirectories.Where(x => x.Value == int.MinValue))
                {
                    _lstCustomDataDirectoryInfo.Add(objLoopPair.Key);
                }

                objCustomDataDirectoryKey.Close();
            }

            XmlManager.RebuildDataDirectoryInfo(_lstCustomDataDirectoryInfo);
        }

        /// <summary>
        /// List of CustomDataDirectoryInfo.
        /// </summary>
        public static IList<CustomDataDirectoryInfo> CustomDataDirectoryPaths => _lstCustomDataDirectoryInfo;

        public static bool OmaeEnabled
        {
            get => _omaeEnabled;
            set => _omaeEnabled = value;
        }

        /// <summary>
        /// Should the updater check for Release builds, or Nightly builds
        /// </summary>
        public static bool PreferNightlyBuilds
        {
            get => _blnPreferNightlyUpdates;
            set => _blnPreferNightlyUpdates = value;
        }

        /// <summary>
        /// Should charts that can cause crash behaviour in Wine be shown
        /// </summary>
        public static bool HideCharts
        {
            get => _blnHideCharts;
            set => _blnHideCharts = value;
        }

        public static string CharacterRosterPath
        {
            get => _strCharacterRosterPath;
            set => _strCharacterRosterPath = value;
        }

        public static string PDFArguments { get; internal set; }
        #endregion

        #region MRU Methods
        private static void LstFavoritedCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for(int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstFavoritedCharacters.Count)
                                _objBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), _lstFavoritedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstFavoritedCharacters.Count)
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
                        if(!string.IsNullOrEmpty(strNewFile))
                            _objBaseChummerKey.SetValue("stickymru" + (e.OldStartingIndex + 1).ToString(InvariantCultureInfo), strNewFile);
                        else
                        {
                            for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if(i <= _lstFavoritedCharacters.Count)
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
                        if(intOldStartingIndex == intNewStartingIndex)
                            break;

                        int intUpdateFrom;
                        int intUpdateTo;
                        if(intOldStartingIndex > intNewStartingIndex)
                        {
                            intUpdateFrom = intNewStartingIndex;
                            intUpdateTo = intOldStartingIndex;
                        }
                        else
                        {
                            intUpdateFrom = intOldStartingIndex;
                            intUpdateTo = intNewStartingIndex;
                        }

                        for(int i = intUpdateFrom; i <= intUpdateTo; ++i)
                        {
                            _objBaseChummerKey.SetValue("stickymru" + (i + 1).ToString(InvariantCultureInfo), _lstFavoritedCharacters[i]);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("stickymru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        for(int i = 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstFavoritedCharacters.Count)
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
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for(int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(), false);
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstMostRecentlyUsedCharacters.Count)
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
                        if(!string.IsNullOrEmpty(strNewFile))
                        {
                            _objBaseChummerKey.SetValue("mru" + (e.OldStartingIndex + 1).ToString(), strNewFile);
                        }
                        else
                        {
                            for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if(i <= _lstMostRecentlyUsedCharacters.Count)
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
                        if(intOldStartingIndex == intNewStartingIndex)
                            break;

                        int intUpdateFrom;
                        int intUpdateTo;
                        if(intOldStartingIndex > intNewStartingIndex)
                        {
                            intUpdateFrom = intNewStartingIndex;
                            intUpdateTo = intOldStartingIndex;
                        }
                        else
                        {
                            intUpdateFrom = intOldStartingIndex;
                            intUpdateTo = intNewStartingIndex;
                        }

                        for(int i = intUpdateFrom; i <= intUpdateTo; ++i)
                        {
                            _objBaseChummerKey.SetValue("mru" + (i + 1).ToString(), _lstMostRecentlyUsedCharacters[i]);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        for(int i = 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstMostRecentlyUsedCharacters.Count)
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
        public static bool CustomDateTimeFormats => _blnCustomDateTimeFormats;
        public static string CustomDateFormat => _strCustomDateFormat;
        public static string CustomTimeFormat => _strCustomTimeFormat;
        /// <summary>
        /// Should the application assume that the Black Market Pipeline discount should automatically be used if the character has an appropriate contact?
        /// </summary>
        public static bool AssumeBlackMarket { get; set; }

        #endregion

    }
}
