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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using iText.Kernel.Pdf;
using Microsoft.Win32;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;
using Xoshiro.PRNG64;

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

    public enum ColorMode
    {
        Automatic = 0,
        Light,
        Dark
    }

    public sealed class SourcebookInfo : IDisposable
    {
        private string _strPath = string.Empty;
        private PdfDocument _objPdfDocument;

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
                    _objPdfDocument?.Close();
                    _objPdfDocument = null;
                }
            }
        }

        public int Offset { get; set; }

        internal PdfDocument CachedPdfDocument
        {
            get
            {
                if (_objPdfDocument == null)
                {
                    Uri uriPath = new Uri(Path);
                    if (File.Exists(uriPath.LocalPath))
                    {
                        _objPdfDocument = new PdfDocument(new PdfReader(uriPath.LocalPath));
                    }
                }
                return _objPdfDocument;
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
                    _objPdfDocument?.Close();
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static CultureInfo s_ObjLanguageCultureInfo = CultureInfo.GetCultureInfo(DefaultLanguage);

        public static StringBuilder ErrorMessage { get; } = new StringBuilder();
        public static event TextEventHandler MRUChanged;
        public static event PropertyChangedEventHandler ClipboardChanged;

        public const int MaxMruSize = 10;
        private static readonly MostRecentlyUsedCollection<string> _lstMostRecentlyUsedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);
        private static readonly MostRecentlyUsedCollection<string> _lstFavoritedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);

        private static readonly RegistryKey _objBaseChummerKey;
        public const string DefaultLanguage = "en-us";
        public const string DefaultCharacterSheetDefaultValue = "Shadowrun 5 (Skills grouped by Rating greater 0)";
        public const string DefaultCharacterOptionDefaultValue = "223a11ff-80e0-428b-89a9-6ef1c243b8b6"; // GUID for built-in Standard option

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
        private static bool _blnPreferNightlyUpdates;
        private static bool _blnLiveUpdateCleanCharacterFiles;
        private static bool _blnHideMasterIndex;
        private static bool _blnHideCharacterRoster;
        private static bool _blnCreateBackupOnCareer;
        private static bool _blnPluginsEnabled;
        private static bool _blnAllowEasterEggs;
        private static bool _blnCustomDateTimeFormats;
        private static string _strCustomDateFormat;
        private static string _strCustomTimeFormat;
        private static string _strDefaultCharacterOption = DefaultCharacterOptionDefaultValue;
        private static int _intSavedImageQuality = int.MaxValue;
        private static ColorMode _eColorMode;
        private static bool _blnConfirmDelete = true;
        private static bool _blnConfirmKarmaExpense = true;
        private static bool _blnHideItemsOverAvailLimit = true;
        private static bool _blnAllowHoverIncrement;
        private static bool _blnSearchInCategoryOnly = true;
        private static bool _blnAllowSkillDiceRolling;

        public const int MaxStackLimit = 1024;
        private static bool _blnShowCharacterCustomDataWarning;

        public static ThreadSafeRandom RandomGenerator { get; } = new ThreadSafeRandom(new XoRoShiRo128starstar());

        // Plugins information
        public static Dictionary<string, bool> PluginsEnabledDic { get; } = new Dictionary<string, bool>();

        // PDF information.
        private static string _strPDFAppPath = string.Empty;
        private static string _strPDFParameters = string.Empty;
        private static HashSet<SourcebookInfo> _lstSourcebookInfo;
        private static bool _blnUseLogging;
        private static UseAILogging _enumUseLoggingApplicationInsights;
        private static string _strCharacterRosterPath;
        private static string _strImageFolder = string.Empty;

        // Custom Data Directory information.
        private static readonly HashSet<CustomDataDirectoryInfo> _setCustomDataDirectoryInfo = new HashSet<CustomDataDirectoryInfo>();

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

            bool blnFirstEverLaunch = false;
            try
            {
                blnFirstEverLaunch = Registry.CurrentUser.OpenSubKey("Software\\Chummer5") == null;
                _objBaseChummerKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            }
            catch (Exception ex)
            {
                if(ErrorMessage.Length > 0)
                    ErrorMessage.AppendLine().AppendLine();
                ErrorMessage.Append(ex);
            }
            if (_objBaseChummerKey == null)
                return;
            _objBaseChummerKey.CreateSubKey("Sourcebook");

            // Automatic Update.
            LoadBoolFromRegistry(ref _blnAutomaticUpdate, "autoupdate");
            LoadBoolFromRegistry(ref _blnLiveCustomData, "livecustomdata");
            LoadBoolFromRegistry(ref _blnLiveUpdateCleanCharacterFiles, "liveupdatecleancharacterfiles");
            LoadBoolFromRegistry(ref _lifeModuleEnabled, "lifemodule");

            // Whether or not the app should use logging.
            LoadBoolFromRegistry(ref _blnUseLogging, "uselogging");

            //Should the App "Phone home"
            try
            {
                string useAI = "NotSet";
                LoadStringFromRegistry(ref useAI, "useloggingApplicationInsights");
                switch (useAI)
                {
                    case "False":
                        _enumUseLoggingApplicationInsights = UseAILogging.NotSet;
                        break;
                    case "True":
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

            string strColorMode = string.Empty;
            if (LoadStringFromRegistry(ref strColorMode, "colormode"))
            {
                switch (strColorMode)
                {
                    case nameof(ColorMode.Light):
                        _eColorMode = ColorMode.Light;
                        break;
                    case nameof(ColorMode.Dark):
                        _eColorMode = ColorMode.Dark;
                        break;
                }
            }
            // In order to not throw off veteran users, forced Light mode is the default for them instead of Automatic
            else if (!blnFirstEverLaunch)
                _eColorMode = ColorMode.Light;

            // Whether or not dates should include the time.
            LoadBoolFromRegistry(ref _blnDatesIncludeTime, "datesincludetime");
            LoadBoolFromRegistry(ref _blnHideMasterIndex, "hidemasterindex");
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

            LoadStringFromRegistry(ref _strDefaultCharacterOption, "defaultcharacteroption"); // Deliberate name change to force users to re-check

            LoadBoolFromRegistry(ref _blnAllowEasterEggs, "alloweastereggs");
            // Confirm delete.
            LoadBoolFromRegistry(ref _blnConfirmDelete, "confirmdelete");
            // Confirm Karma Expense.
            LoadBoolFromRegistry(ref _blnConfirmKarmaExpense, "confirmkarmaexpense");
            LoadBoolFromRegistry(ref _blnHideItemsOverAvailLimit, "hideitemsoveravaillimit");
            LoadBoolFromRegistry(ref _blnAllowHoverIncrement, "allowhoverincrement");
            LoadBoolFromRegistry(ref _blnSearchInCategoryOnly, "searchincategoryonly");
            // Whether or not dice rolling is allowed for Skills.
            LoadBoolFromRegistry(ref _blnAllowSkillDiceRolling, "allowskilldicerolling");

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

            // Most recent image folder location used.
            LoadStringFromRegistry(ref _strImageFolder, "recentimagefolder");

            // Which Plugins are enabled.
            LoadBoolFromRegistry(ref _blnPluginsEnabled, "pluginsenabled");

            try
            {
                string jsonstring = string.Empty;
                LoadStringFromRegistry(ref jsonstring, "plugins");
                if(!string.IsNullOrEmpty(jsonstring))
                    PluginsEnabledDic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(jsonstring);

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
#if DEBUG
                throw;
/*
*#else
*                string msg = "Error while loading PluginOptions from registry: " + Environment.NewLine;
*                msg += e.Message;
*                Program.MainForm.ShowMessageBox(msg);
*/
#endif
            }

            // Prefer Nightly Updates.
            LoadBoolFromRegistry(ref _blnPreferNightlyUpdates, "prefernightlybuilds");

            LoadBoolFromRegistry(ref _blnCustomDateTimeFormats, "usecustomdatetime");
            LoadStringFromRegistry(ref _strCustomDateFormat, "customdateformat");
            LoadStringFromRegistry(ref _strCustomTimeFormat, "customtimeformat");

            // The quality at which images should be saved. int.MaxValue saves as Png, everything else saves as Jpeg
            LoadInt32FromRegistry(ref _intSavedImageQuality, "savedimagequality");

            // Retrieve CustomDataDirectoryInfo objects from registry
            RegistryKey objCustomDataDirectoryKey = _objBaseChummerKey.OpenSubKey("CustomDataDirectory");
            if (objCustomDataDirectoryKey != null)
            {
                foreach (string strDirectoryName in objCustomDataDirectoryKey.GetSubKeyNames())
                {
                    using (RegistryKey objLoopKey = objCustomDataDirectoryKey.OpenSubKey(strDirectoryName))
                    {
                        if (objLoopKey == null)
                            continue;
                        string strPath = string.Empty;
                        object objRegistryResult = objLoopKey.GetValue("Path");
                        if (objRegistryResult != null)
                            strPath = objRegistryResult.ToString().Replace("$CHUMMER", Utils.GetStartupPath);
                        if (!string.IsNullOrEmpty(strPath) && Directory.Exists(strPath))
                        {
                            CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo(strDirectoryName, strPath);
                            _setCustomDataDirectoryInfo.Add(objCustomDataDirectory);
                        }
                    }
                }

                objCustomDataDirectoryKey.Close();
            }
            // Add in default customdata directory's paths
            string strCustomDataRootPath = Path.Combine(Utils.GetStartupPath, "customdata");
            if (Directory.Exists(strCustomDataRootPath))
            {
                foreach (string strLoopDirectoryPath in Directory.GetDirectories(strCustomDataRootPath))
                {
                    // Only add directories for which we don't already have entries loaded from registry
                    if (_setCustomDataDirectoryInfo.All(x => x.Path != strLoopDirectoryPath))
                    {
                        CustomDataDirectoryInfo objCustomDataDirectory = new CustomDataDirectoryInfo(Path.GetFileName(strLoopDirectoryPath), strLoopDirectoryPath);
                        _setCustomDataDirectoryInfo.Add(objCustomDataDirectory);
                    }
                }
            }

            XmlManager.RebuildDataDirectoryInfo(_setCustomDataDirectoryInfo);

            for (int i = 1; i <= MaxMruSize; i++)
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
            
            if (blnFirstEverLaunch)
                ShowCharacterCustomDataWarning = false;
            else
            {
                bool blnTemp = false;
                LoadBoolFromRegistry(ref blnTemp, "charactercustomdatawarningshown");
                ShowCharacterCustomDataWarning = !blnTemp;
            }
        }
        #endregion

        #region Methods

        public static void SaveOptionsToRegistry()
        {
            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
            {
                if (objRegistry == null)
                    return;
                objRegistry.SetValue("autoupdate", AutomaticUpdate.ToString(InvariantCultureInfo));
                objRegistry.SetValue("livecustomdata", LiveCustomData.ToString(InvariantCultureInfo));
                objRegistry.SetValue("liveupdatecleancharacterfiles", LiveUpdateCleanCharacterFiles.ToString(InvariantCultureInfo));
                objRegistry.SetValue("uselogging", UseLogging.ToString(InvariantCultureInfo));
                objRegistry.SetValue("useloggingApplicationInsights", UseLoggingApplicationInsights.ToString());
                objRegistry.SetValue("colormode", ColorModeSetting.ToString());
                objRegistry.SetValue("language", Language);
                objRegistry.SetValue("startupfullscreen", StartupFullscreen.ToString(InvariantCultureInfo));
                objRegistry.SetValue("singlediceroller", SingleDiceRoller.ToString(InvariantCultureInfo));
                objRegistry.SetValue("defaultsheet", DefaultCharacterSheet);
                objRegistry.SetValue("datesincludetime", DatesIncludeTime.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printtofilefirst", PrintToFileFirst.ToString(InvariantCultureInfo));
                objRegistry.SetValue("emulatedbrowserversion", EmulatedBrowserVersion.ToString(InvariantCultureInfo));
                objRegistry.SetValue("pdfapppath", PDFAppPath);
                objRegistry.SetValue("pdfparameters", PDFParameters);
                objRegistry.SetValue("lifemodule", LifeModuleEnabled.ToString(InvariantCultureInfo));
                objRegistry.SetValue("prefernightlybuilds", PreferNightlyBuilds.ToString(InvariantCultureInfo));
                objRegistry.SetValue("characterrosterpath", CharacterRosterPath);
                objRegistry.SetValue("hidemasterindex", HideMasterIndex.ToString(InvariantCultureInfo));
                objRegistry.SetValue("hidecharacterroster", HideCharacterRoster.ToString(InvariantCultureInfo));
                objRegistry.SetValue("createbackuponcareer", CreateBackupOnCareer.ToString(InvariantCultureInfo));
                objRegistry.SetValue("confirmdelete", ConfirmDelete.ToString(InvariantCultureInfo));
                objRegistry.SetValue("confirmkarmaexpense", ConfirmKarmaExpense.ToString(InvariantCultureInfo));
                objRegistry.SetValue("hideitemsoveravaillimit", HideItemsOverAvailLimit.ToString(InvariantCultureInfo));
                objRegistry.SetValue("allowhoverincrement", AllowHoverIncrement.ToString(InvariantCultureInfo));
                objRegistry.SetValue("searchincategoryonly", SearchInCategoryOnly.ToString(InvariantCultureInfo));
                objRegistry.SetValue("allowskilldicerolling", AllowSkillDiceRolling.ToString(InvariantCultureInfo));
                objRegistry.SetValue("pluginsenabled", PluginsEnabled.ToString(InvariantCultureInfo));
                objRegistry.SetValue("alloweastereggs", AllowEasterEggs.ToString(InvariantCultureInfo));
                objRegistry.SetValue("defaultcharacteroption", DefaultCharacterOption);
                objRegistry.SetValue("usecustomdatetime", CustomDateTimeFormats.ToString(InvariantCultureInfo));
                objRegistry.SetValue("customdateformat", CustomDateFormat);
                objRegistry.SetValue("customtimeformat", CustomTimeFormat);
                objRegistry.SetValue("savedimagequality", SavedImageQuality.ToString(InvariantCultureInfo));

                //Save the Plugins-Dictionary
                objRegistry.SetValue("plugins", Newtonsoft.Json.JsonConvert.SerializeObject(PluginsEnabledDic));

                // Save the SourcebookInfo.
                using (RegistryKey objSourceRegistry = objRegistry.CreateSubKey("Sourcebook"))
                {
                    if (objSourceRegistry != null)
                    {
                        foreach (SourcebookInfo objSource in SourcebookInfo)
                            objSourceRegistry.SetValue(objSource.Code, objSource.Path + '|' + objSource.Offset.ToString(InvariantCultureInfo));
                    }
                }

                // Save the Custom Data Directory Info.
                if (objRegistry.OpenSubKey("CustomDataDirectory") != null)
                    objRegistry.DeleteSubKeyTree("CustomDataDirectory");
                using (RegistryKey objCustomDataDirectoryRegistry = objRegistry.CreateSubKey("CustomDataDirectory"))
                {
                    if (objCustomDataDirectoryRegistry != null)
                    {
                        foreach (CustomDataDirectoryInfo objCustomDataDirectory in CustomDataDirectoryInfos)
                        {
                            using (RegistryKey objLoopKey = objCustomDataDirectoryRegistry.CreateSubKey(objCustomDataDirectory.Name))
                            {
                                objLoopKey?.SetValue("Path", objCustomDataDirectory.Path.Replace(Utils.GetStartupPath, "$CHUMMER"));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not to create backups of characters before moving them to career mode. If true, a separate save file is created before marking the current character as created.
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
        /// Whether or not the Master Index should be shown. If true, prevents the roster from being removed or hidden.
        /// </summary>
        public static bool HideMasterIndex
        {
            get => _blnHideMasterIndex;
            set => _blnHideMasterIndex = value;
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
        /// Whether or not confirmation messages are shown when deleting an object.
        /// </summary>
        public static bool ConfirmDelete
        {
            get => _blnConfirmDelete;
            set => _blnConfirmDelete = value;
        }

        /// <summary>
        /// Whether or not confirmation messages are shown for Karma Expenses.
        /// </summary>
        public static bool ConfirmKarmaExpense
        {
            get => _blnConfirmKarmaExpense;
            set => _blnConfirmKarmaExpense = value;
        }

        /// <summary>
        /// Whether items that exceed the Availability Limit should be shown in Create Mode.
        /// </summary>
        public static bool HideItemsOverAvailLimit
        {
            get => _blnHideItemsOverAvailLimit;
            set => _blnHideItemsOverAvailLimit = value;
        }

        /// <summary>
        /// Whether or not numeric updowns can increment values of numericupdown controls by hovering over the control.
        /// </summary>
        public static bool AllowHoverIncrement
        {
            get => _blnAllowHoverIncrement;
            set => _blnAllowHoverIncrement = value;
        }

        /// <summary>
        /// Whether searching in a selection form will limit itself to the current Category that's selected.
        /// </summary>
        public static bool SearchInCategoryOnly
        {
            get => _blnSearchInCategoryOnly;
            set => _blnSearchInCategoryOnly = value;
        }

        public static NumericUpDownEx.InterceptMouseWheelMode InterceptMode => AllowHoverIncrement ? NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver : NumericUpDownEx.InterceptMouseWheelMode.WhenFocus;

        /// <summary>
        /// Whether or not dice rolling is allowed for Skills.
        /// </summary>
        public static bool AllowSkillDiceRolling
        {
            get => _blnAllowSkillDiceRolling;
            set => _blnAllowSkillDiceRolling = value;
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
                        LogManager.EnableLogging();
                    else
                        LogManager.DisableLogging();
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
        /// Whether the program should be forced to use Light/Dark mode or to obey default color schemes automatically
        /// </summary>
        public static ColorMode ColorModeSetting
        {
            get => _eColorMode;
            set
            {
                if (_eColorMode == value)
                    return;
                _eColorMode = value;
                switch (value)
                {
                    case ColorMode.Automatic:
                        ColorManager.AutoApplyLightDarkMode();
                        break;
                    case ColorMode.Light:
                        ColorManager.DisableAutoTimer();
                        ColorManager.IsLightMode = true;
                        break;
                    case ColorMode.Dark:
                        ColorManager.DisableAutoTimer();
                        ColorManager.IsLightMode = false;
                        break;
                }
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
                    // Set default cultures based on the currently set language
                    CultureInfo.DefaultThreadCurrentCulture = s_ObjLanguageCultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = s_ObjLanguageCultureInfo;
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
        public static CultureInfo InvariantCultureInfo => CultureInfo.InvariantCulture;

        /// <summary>
        /// CultureInfo of the user's current system.
        /// </summary>
        public static CultureInfo SystemCultureInfo => CultureInfo.CurrentCulture;

        private static XmlDocument _xmlClipboard = new XmlDocument {XmlResolver = null};

        public static XmlReaderSettings SafeXmlReaderSettings { get; } = new XmlReaderSettings {XmlResolver = null};
        /// <summary>
        /// XmlReaderSettings that should only be used if invalid characters are found. 
        /// </summary>
        public static XmlReaderSettings UnSafeXmlReaderSettings { get; } = new XmlReaderSettings { XmlResolver = null, CheckCharacters = false };
        /// <summary>
        /// Regex that indicates whether a given string is a match for text that cannot be saved in XML. Match == true.
        /// </summary>
        public static string InvalidXmlCharacterRegex = "[\u0000-\u0008\u000B\u000C\u000E-\u001F]";

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
        /// Default character option to select when creating a new character
        /// </summary>
        public static string DefaultCharacterOption
        {
            get => _strDefaultCharacterOption;
            set => _strDefaultCharacterOption = value;
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
        public static HashSet<SourcebookInfo> SourcebookInfo
        {
            get
            {
                // We need to generate _lstSourcebookInfo outside of the constructor to avoid initialization cycles
                if(_lstSourcebookInfo == null)
                {
                    _lstSourcebookInfo = new HashSet<SourcebookInfo>();
                    // Retrieve the SourcebookInfo objects.
                    using (XmlNodeList xmlBookList = XmlManager.Load("books.xml").SelectNodes("/chummer/books/book"))
                    {
                        if (xmlBookList != null)
                        {
                            foreach (XmlNode xmlBook in xmlBookList)
                            {
                                string strCode = xmlBook["code"]?.InnerText;
                                if (string.IsNullOrEmpty(strCode))
                                    continue;
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
                                        if (string.IsNullOrEmpty(objSource.Path))
                                        {
                                            objSource.Path = string.Empty;
                                            objSource.Offset = 0;
                                        }
                                        else
                                        {
                                            if (!File.Exists(objSource.Path))
                                                objSource.Path = string.Empty;
                                            if (strParts.Length > 1 && int.TryParse(strParts[1], out int intTmp))
                                            {
                                                objSource.Offset = intTmp;
                                            }
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
                }
                return _lstSourcebookInfo;
            }
        }

        /// <summary>
        /// List of CustomDataDirectoryInfo.
        /// </summary>
        public static HashSet<CustomDataDirectoryInfo> CustomDataDirectoryInfos => _setCustomDataDirectoryInfo;

        /// <summary>
        /// Should the updater check for Release builds, or Nightly builds
        /// </summary>
        public static bool PreferNightlyBuilds
        {
            get => _blnPreferNightlyUpdates;
            set => _blnPreferNightlyUpdates = value;
        }

        /// <summary>
        /// Whether or not to show a warning that this Nightly build is special
        /// </summary>
        public static bool ShowCharacterCustomDataWarning
        {
            get => _blnShowCharacterCustomDataWarning;
            set
            {
                _blnShowCharacterCustomDataWarning = value;
                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
                {
                    if (objRegistry == null)
                        return;
                    if (value)
                    {
                        objRegistry.DeleteValue("charactercustomdatawarningshown");
                    }
                    else
                    {
                        objRegistry.SetValue("charactercustomdatawarningshown", bool.TrueString);
                    }
                }
            }
        }

        public static string CharacterRosterPath
        {
            get => _strCharacterRosterPath;
            set => _strCharacterRosterPath = value;
        }

        public static string PDFArguments { get; internal set; }

        public static int SavedImageQuality
        {
            get => _intSavedImageQuality;
            set => _intSavedImageQuality = value;
        }

        public static string ImageToBase64StringForStorage(Image objImageToSave)
        {
            return SavedImageQuality == int.MaxValue
                ? objImageToSave.ToBase64String(ImageFormat.Png)
                : objImageToSave.ToBase64StringAsJpeg(SavedImageQuality);
        }

        /// <summary>
        /// Last folder from which a mugshot was added
        /// </summary>
        public static string RecentImageFolder
        {
            get => _strImageFolder;
            set
            {
                if (_strImageFolder != value)
                {
                    _strImageFolder = value;
                    using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
                        objRegistry?.SetValue("recentimagefolder", value);
                }
            }
        }
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
                                _objBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }

                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strNewFile = e.NewItems.Count > 0 ? e.NewItems[0] as string : string.Empty;
                        if(!string.IsNullOrEmpty(strNewFile))
                        {
                            _objBaseChummerKey.SetValue("mru" + (e.OldStartingIndex + 1).ToString(InvariantCultureInfo), strNewFile);
                        }
                        else
                        {
                            for(int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if(i <= _lstMostRecentlyUsedCharacters.Count)
                                    _objBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo), _lstMostRecentlyUsedCharacters[i - 1]);
                                else
                                    _objBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
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
                            _objBaseChummerKey.SetValue("mru" + (i + 1).ToString(InvariantCultureInfo), _lstMostRecentlyUsedCharacters[i]);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        for(int i = 1; i <= MaxMruSize; ++i)
                        {
                            if(i <= _lstMostRecentlyUsedCharacters.Count)
                                _objBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo), _lstMostRecentlyUsedCharacters[i - 1]);
                            else
                                _objBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }
                        MRUChanged?.Invoke(sender, new TextEventArgs("mru"));
                        break;
                    }
            }
        }

        public static ObservableCollection<string> FavoritedCharacters => _lstFavoritedCharacters;

        public static ObservableCollection<string> MostRecentlyUsedCharacters => _lstMostRecentlyUsedCharacters;

        public static bool CustomDateTimeFormats
        {
            get => _blnCustomDateTimeFormats;
            set => _blnCustomDateTimeFormats = value;
        }

        public static string CustomDateFormat
        {
            get => _strCustomDateFormat;
            set => _strCustomDateFormat = value;
        }

        public static string CustomTimeFormat
        {
            get => _strCustomTimeFormat;
            set => _strCustomTimeFormat = value;
        }

        /// <summary>
        /// Should the application assume that the Black Market Pipeline discount should automatically be used if the character has an appropriate contact?
        /// </summary>
        public static bool AssumeBlackMarket { get; set; }

        #endregion

    }
}
