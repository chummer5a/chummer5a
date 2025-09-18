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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using iText.Kernel.Pdf;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Win32;
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

    public enum DpiScalingMethod
    {
        None = 0,
        Zoom,       // System
        Rescale,    // PerMonitor/PerMonitorV2
        SmartZoom   // System (Enhanced)
    }

    public sealed class SourcebookInfo : IDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private string _strPath = string.Empty;
        private PdfReader _objPdfReader;
        private PdfDocument _objPdfDocument;

        public SourcebookInfo()
        {
        }

        /// <summary>
        /// Special constructor used when we have already created a PdfReader assigned to a file representing this SourcebookInfo.
        /// </summary>
        /// <param name="strPath">Path to the file.</param>
        /// <param name="objPdfReader">PdfReader object associated with the file.</param>
        [CLSCompliant(false)]
        public SourcebookInfo(string strPath, PdfReader objPdfReader)
        {
            if (string.IsNullOrEmpty(strPath))
                throw new ArgumentNullException(nameof(strPath));
            _strPath = strPath;
            _objPdfReader = objPdfReader ?? throw new ArgumentNullException(nameof(objPdfReader));
            _objPdfDocument = new PdfDocument(objPdfReader);
        }

        /// <summary>
        /// Special constructor used when we have already created a PdfDocument assigned to a file representing this SourcebookInfo.
        /// </summary>
        /// <param name="strPath">Path to the file.</param>
        /// <param name="objPdfDocument">PdfDocument object associated with the file.</param>
        [CLSCompliant(false)]
        public SourcebookInfo(string strPath, PdfDocument objPdfDocument)
        {
            if (string.IsNullOrEmpty(strPath))
                throw new ArgumentNullException(nameof(strPath));
            _strPath = strPath;
            _objPdfReader = objPdfDocument.GetReader() ?? throw new ArgumentException("objPdfDocument has no associated reader", nameof(objPdfDocument));
            _objPdfDocument = objPdfDocument;
        }

        /// <summary>
        /// Special constructor used when we have already created a PdfReader and PdfDocument assigned to a file representing this SourcebookInfo
        /// </summary>
        /// <param name="strPath">Path to the file.</param>
        /// <param name="objPdfReader">PdfReader object associated with the file.</param>
        /// <param name="objPdfDocument">PdfDocument object associated with the file.</param>
        /// <exception cref="ArgumentException"><paramref name="objPdfDocument"/>'s associated reader is not the same value as<paramref name="objPdfReader"/>.</exception>
        [CLSCompliant(false)]
        public SourcebookInfo(string strPath, PdfReader objPdfReader, PdfDocument objPdfDocument)
        {
            if (string.IsNullOrEmpty(strPath))
                throw new ArgumentNullException(nameof(strPath));
            _strPath = strPath;
            _objPdfReader = objPdfReader ?? throw new ArgumentNullException(nameof(objPdfReader));
            if (objPdfDocument?.GetReader() != objPdfReader)
                throw new ArgumentException("objPdfDocument reader is different from objPdfReader", nameof(objPdfDocument));
            _objPdfDocument = objPdfDocument;
        }

        #region Properties

        public string Code { get; set; } = string.Empty;

        public string Path
        {
            get => _strPath;
            set
            {
                if (Interlocked.Exchange(ref _strPath, value) == value)
                    return;
                Interlocked.Exchange(ref _objPdfDocument, null)?.Close();
                Interlocked.Exchange(ref _objPdfReader, null)?.Close();
            }
        }

        public int Offset { get; set; }

        internal PdfDocument CachedPdfDocument
        {
            get
            {
                if (_objPdfDocument == null)
                {
                    string strPath = new Uri(Path).LocalPath;
                    if (File.Exists(strPath))
                    {
                        try
                        {
                            PdfReader objReader = new PdfReader(strPath);
                            PdfDocument objReturn = new PdfDocument(objReader);
                            Interlocked.Exchange(ref _objPdfDocument, objReturn)?.Close();
                            Interlocked.Exchange(ref _objPdfReader, objReader)?.Close();
                            return objReturn;
                        }
                        catch (Exception e)
                        {
                            e = e.Demystify();
                            Log.Warn(e, $"Exception while loading {strPath}: " + e.Message);
                            Interlocked.Exchange(ref _objPdfDocument, null)?.Close();
                            Interlocked.Exchange(ref _objPdfReader, null)?.Close();
                        }
                    }
                }
                return _objPdfDocument;
            }
        }

        #region IDisposable Support

        private int _intIsDisposed; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (disposing && Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) == 0)
            {
                _objPdfDocument?.Close();
                _objPdfReader?.Close();
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion IDisposable Support

        #endregion Properties
    }

    /// <summary>
    /// Global Settings. A single instance class since Settings are common for all characters, reduces execution time and memory usage.
    /// </summary>
    public static class GlobalSettings
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private static CultureInfo _objLanguageCultureInfo = CultureInfo.GetCultureInfo(DefaultLanguage);

        public static string ErrorMessage { get; }

        public static event EventHandler<TextEventArgs> MruChanged;

        public static event PropertyChangedEventHandler ClipboardChanged;

        public static event PropertyChangedAsyncEventHandler ClipboardChangedAsync;

        public const int MaxMruSize = 10;
        private static readonly MostRecentlyUsedCollection<string> s_LstMostRecentlyUsedCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);
        private static readonly MostRecentlyUsedCollection<string> s_LstFavoriteCharacters = new MostRecentlyUsedCollection<string>(MaxMruSize);

        private static readonly RegistryKey s_ObjBaseChummerKey;
        public const string DefaultLanguage = "en-us";
        public const string DefaultCharacterSheetDefaultValue = "Shadowrun 5 (Skills grouped by Rating greater 0)";
        public const string DefaultCharacterSettingDefaultValue = "223a11ff-80e0-428b-89a9-6ef1c243b8b6"; // GUID for built-in Standard option
        public const string DefaultMasterIndexSettingDefaultValue = "67e25032-2a4e-42ca-97fa-69f7f608236c"; // GUID for built-in Full House option
        public const DpiScalingMethod DefaultDpiScalingMethod = DpiScalingMethod.Zoom;
        public const LzmaHelper.ChummerCompressionPreset DefaultChum5lzCompressionLevel
            = LzmaHelper.ChummerCompressionPreset.Balanced;

        private static DpiScalingMethod _eDpiScalingMethod = DefaultDpiScalingMethod;

        private static bool _blnAutomaticUpdate;
        private static bool _blnLiveCustomData;
        private static bool _blnStartupFullscreen;
        private static bool _blnSingleDiceRoller = true;
        private static string _strLanguage = DefaultLanguage;
        private static string _strDefaultCharacterSheet = DefaultCharacterSheetDefaultValue;
        private static bool _blnDatesIncludeTime = true;
        private static bool _blnPrintToFileFirst;
        private static int _intEmulatedBrowserVersion = 11;
        private static bool _lifeModuleEnabled;
        private static bool _blnPreferNightlyUpdates = !Utils.IsMilestoneVersion;
        private static bool _blnLiveUpdateCleanCharacterFiles;
        private static bool _blnHideMasterIndex;
        private static bool _blnHideCharacterRoster;
        private static bool _blnCreateBackupOnCareer;
        private static bool _blnPluginsEnabled;
        private static bool _blnAllowEasterEggs;
        private static bool _blnCustomDateTimeFormats;
        private static LzmaHelper.ChummerCompressionPreset _eChum5lzCompressionLevel = DefaultChum5lzCompressionLevel; // Level of compression to use for .chum5lz files
        private static string _strCustomDateFormat;
        private static string _strCustomTimeFormat;
        private static string _strDefaultCharacterSetting = DefaultCharacterSettingDefaultValue;
        private static string _strDefaultMasterIndexSetting = DefaultMasterIndexSettingDefaultValue;
        private static int _intSavedImageQuality = -1; // Jpeg compression with automatic quality
        private static ColorMode _eColorMode;
        private static Color _objDefaultHasNotesColor = Color.Chocolate;
        private static bool _blnConfirmDelete = true;
        private static bool _blnConfirmKarmaExpense = true;
        private static bool _blnHideItemsOverAvailLimit = true;
        private static bool _blnAllowHoverIncrement;
        private static bool _blnSwitchTabsOnHoverScroll;
        private static bool _blnSearchInCategoryOnly = true;
        private static bool _blnAllowSkillDiceRolling;
        private static bool _blnPrintExpenses;
        private static bool _blnPrintFreeExpenses = true;
        private static bool _blnPrintNotes;
        private static bool _blnPrintSkillsWithZeroRating = true;
        private static bool _blnInsertPdfNotesIfAvailable = true;

        /// <summary>
        /// Maximum size of a stackalloc'ed array for an 8-bit type (byte, sbyte, bool)
        /// </summary>
        public const int MaxStackLimit8BitTypes = 4096;
        /// <summary>
        /// Maximum size of a stackalloc'ed array for a 16-bit type (short, ushort, char)
        /// </summary>
        public const int MaxStackLimit16BitTypes = MaxStackLimit8BitTypes * sizeof(byte) / sizeof(short);
        /// <summary>
        /// Maximum size of a stackalloc'ed array for a 32-bit type (int, uint, float)
        /// </summary>
        public const int MaxStackLimit32BitTypes = MaxStackLimit8BitTypes * sizeof(byte) / sizeof(int);
        /// <summary>
        /// Maximum size of a stackalloc'ed array for a 64-bit type (long, ulong, double)
        /// </summary>
        public const int MaxStackLimit64BitTypes = MaxStackLimit8BitTypes * sizeof(byte) / sizeof(long);
        /// <summary>
        /// Maximum size of a stackalloc'ed array for a 128-bit type (decimal)
        /// </summary>
        public const int MaxStackLimit128BitTypes = MaxStackLimit8BitTypes * sizeof(byte) / sizeof(decimal);

        private static bool _blnShowCharacterCustomDataWarning;

        public static ThreadSafeCachedRandom RandomGenerator { get; } = new ThreadSafeCachedRandom(new XoRoShiRo128starstar(), true);

        // Plugins information
        private static readonly ConcurrentDictionary<string, bool> s_dicPluginsEnabled = new ConcurrentDictionary<string, bool>();

        public static ConcurrentDictionary<string, bool> PluginsEnabledDic => s_dicPluginsEnabled;

        // PDF information.
        private static string _strPdfAppPath = string.Empty;

        private static string _strPdfParameters = string.Empty;
        private static readonly ConcurrentDictionary<string, SourcebookInfo> s_DicSourcebookInfos = new ConcurrentDictionary<string, SourcebookInfo>();
        private static readonly ConcurrentStringHashSet s_DicCustomSourcebookCodes = new ConcurrentStringHashSet();
        private static int s_intSourcebookInfosLoadingStatus;
        private static bool _blnUseLogging;
        private static UseAILogging _eUseLoggingApplicationInsights;
        private static int _intResetLogging;
        private static string _strCharacterRosterPath;
        private static string _strImageFolder = string.Empty;

        // Custom Data Directory information.
        private static readonly HashSet<CustomDataDirectoryInfo> s_SetCustomDataDirectoryInfos = new HashSet<CustomDataDirectoryInfo>();

        #region Constructor

        /// <summary>
        /// Load a Bool Option from the Registry.
        /// </summary>
        public static bool LoadBoolFromRegistry(ref bool blnStorage, string strBoolName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = string.IsNullOrWhiteSpace(strSubKey)
                ? s_ObjBaseChummerKey
                : s_ObjBaseChummerKey.OpenSubKey(strSubKey);
            if (objKey == null)
                return false;
            try
            {
                object objRegistryResult = objKey.GetValue(strBoolName);
                if (objRegistryResult != null)
                {
                    if (bool.TryParse(objRegistryResult.ToString(), out bool blnTemp))
                        blnStorage = blnTemp;
                    if (blnDeleteAfterFetch)
                        objKey.DeleteValue(strBoolName, false);
                    return true;
                }
            }
            finally
            {
                if (objKey != s_ObjBaseChummerKey)
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load an Int Option from the Registry.
        /// </summary>
        public static bool LoadInt32FromRegistry(ref int intStorage, string strIntName, string strSubKey = "",
                                                 bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = string.IsNullOrWhiteSpace(strSubKey)
                ? s_ObjBaseChummerKey
                : s_ObjBaseChummerKey.OpenSubKey(strSubKey);
            if (objKey == null)
                return false;
            try
            {
                object objRegistryResult = objKey.GetValue(strIntName);
                if (objRegistryResult != null)
                {
                    if (int.TryParse(objRegistryResult.ToString(), out int intTemp))
                        intStorage = intTemp;
                    if (blnDeleteAfterFetch)
                        objKey.DeleteValue(strIntName, false);
                    return true;
                }
            }
            finally
            {
                if (objKey != s_ObjBaseChummerKey)
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load a Decimal Option from the Registry.
        /// </summary>
        public static bool LoadDecFromRegistry(ref decimal decStorage, string strDecName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = string.IsNullOrWhiteSpace(strSubKey)
                ? s_ObjBaseChummerKey
                : s_ObjBaseChummerKey.OpenSubKey(strSubKey);
            if (objKey == null)
                return false;
            try
            {
                object objRegistryResult = objKey.GetValue(strDecName);
                if (objRegistryResult != null)
                {
                    if (decimal.TryParse(objRegistryResult.ToString(), NumberStyles.Any, InvariantCultureInfo, out decimal decTemp))
                        decStorage = decTemp;
                    if (blnDeleteAfterFetch)
                        objKey.DeleteValue(strDecName, false);
                    return true;
                }
            }
            finally
            {
                if (objKey != s_ObjBaseChummerKey)
                    objKey.Close();
            }

            return false;
        }

        /// <summary>
        /// Load an String Option from the Registry.
        /// </summary>
        public static bool LoadStringFromRegistry(ref string strStorage, string strStringName, string strSubKey = "", bool blnDeleteAfterFetch = false)
        {
            RegistryKey objKey = string.IsNullOrWhiteSpace(strSubKey)
                ? s_ObjBaseChummerKey
                : s_ObjBaseChummerKey.OpenSubKey(strSubKey);
            if (objKey == null)
                return false;
            try
            {
                object objRegistryResult = objKey.GetValue(strStringName);
                if (objRegistryResult != null)
                {
                    strStorage = objRegistryResult.ToString();
                    if (blnDeleteAfterFetch)
                        objKey.DeleteValue(strStringName, false);
                    return true;
                }
            }
            finally
            {
                if (objKey != s_ObjBaseChummerKey)
                    objKey.Close();
            }

            return false;
        }

        static GlobalSettings()
        {
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            bool blnFirstEverLaunch = false;
            try
            {
                using (RegistryKey objKey = Registry.CurrentUser.OpenSubKey("Software\\Chummer5"))
                    blnFirstEverLaunch = objKey == null;
                s_ObjBaseChummerKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5", true);
            }
            catch (Exception ex)
            {
                ErrorMessage += ex;
            }
            if (s_ObjBaseChummerKey == null)
                return;

            // Automatic Update.
            LoadBoolFromRegistry(ref _blnAutomaticUpdate, "autoupdate");
            LoadBoolFromRegistry(ref _blnLiveCustomData, "livecustomdata");
            LoadBoolFromRegistry(ref _blnLiveUpdateCleanCharacterFiles, "liveupdatecleancharacterfiles");
            LoadBoolFromRegistry(ref _lifeModuleEnabled, "lifemodule");

            // Whether the app should use logging.
            LoadBoolFromRegistry(ref _blnUseLogging, "uselogging");

            try
            {
                string strDpiScalingMethod = DefaultDpiScalingMethod.ToString();
                LoadStringFromRegistry(ref strDpiScalingMethod, "dpiscalingmethod");
                _eDpiScalingMethod = (DpiScalingMethod)Enum.Parse(typeof(DpiScalingMethod), strDpiScalingMethod);
            }
            catch (Exception e)
            {
                Log.Warn(e);
                _eDpiScalingMethod = DefaultDpiScalingMethod;
            }

            //Should the App "Phone home"
            try
            {
                string useAI = "NotSet";
                LoadStringFromRegistry(ref useAI, "useloggingApplicationInsights");
                switch (useAI)
                {
                    case "False":
                        _eUseLoggingApplicationInsights = UseAILogging.NotSet;
                        break;

                    case "True":
                    case "Yes":
                        _eUseLoggingApplicationInsights = UseAILogging.Info;
                        break;

                    default:
                        _eUseLoggingApplicationInsights = (UseAILogging)Enum.Parse(typeof(UseAILogging), useAI);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                _eUseLoggingApplicationInsights = UseAILogging.NotSet;
            }

            if (LoadInt32FromRegistry(ref _intResetLogging, "useloggingApplicationInsightsResetCounter") && _intResetLogging != 0)
            {
                _intResetLogging--;
                try
                {
                    using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
                    {
                        if (objRegistry == null)
                            return;
                        objRegistry.SetValue("useloggingApplicationInsightsResetCounter", UseLoggingResetCounter);
                    }
                }
                catch (System.Security.SecurityException)
                {
                    Program.ShowScrollableMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning_Registry"));
                }
                catch (UnauthorizedAccessException)
                {
                    Program.ShowScrollableMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning_Registry"));
                }
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

            int intColor = -1;
            if (LoadInt32FromRegistry(ref intColor, "defaulthasnotescolor"))
                _objDefaultHasNotesColor = Color.FromArgb(intColor);
            else
                _objDefaultHasNotesColor = Color.Chocolate;

            // Whether dates should include the time.
            LoadBoolFromRegistry(ref _blnDatesIncludeTime, "datesincludetime");
            LoadBoolFromRegistry(ref _blnHideMasterIndex, "hidemasterindex");
            LoadBoolFromRegistry(ref _blnHideCharacterRoster, "hidecharacterroster");
            LoadBoolFromRegistry(ref _blnCreateBackupOnCareer, "createbackuponcareer");

            // Whether printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
            LoadBoolFromRegistry(ref _blnPrintToFileFirst, "printtofilefirst");

            // Print all Active Skills with a total value greater than 0 (as opposed to only printing those with a Rating higher than 0).
            LoadBoolFromRegistry(ref _blnPrintSkillsWithZeroRating, "printzeroratingskills");

            // Print Expenses.
            LoadBoolFromRegistry(ref _blnPrintExpenses, "printexpenses");

            // Print Free Expenses.
            LoadBoolFromRegistry(ref _blnPrintFreeExpenses, "printfreeexpenses");

            // Print Notes.
            LoadBoolFromRegistry(ref _blnPrintNotes, "printnotes");

            // Whether to insert scraped text from PDFs into the notes fields of newly added items
            LoadBoolFromRegistry(ref _blnInsertPdfNotesIfAvailable, "insertpdfnotesifavailable");

            // Which version of the Internet Explorer's rendering engine will be emulated for rendering the character view.
            LoadInt32FromRegistry(ref _intEmulatedBrowserVersion, "emulatedbrowserversion");
            Utils.SetupWebBrowserRegistryKeys();

            // Default character sheet.
            LoadStringFromRegistry(ref _strDefaultCharacterSheet, "defaultsheet");
            if (_strDefaultCharacterSheet == "Shadowrun (Rating greater 0)")
                _strDefaultCharacterSheet = DefaultCharacterSheetDefaultValue;

            if (!LoadStringFromRegistry(ref _strDefaultCharacterSetting, "defaultcharactersetting"))
                LoadStringFromRegistry(ref _strDefaultCharacterSetting, "defaultcharacteroption"); // Deliberate name change to force users to re-check

            LoadStringFromRegistry(ref _strDefaultMasterIndexSetting, "defaultmasterindexsetting");

            LoadBoolFromRegistry(ref _blnAllowEasterEggs, "alloweastereggs");
            // Confirm delete.
            LoadBoolFromRegistry(ref _blnConfirmDelete, "confirmdelete");
            // Confirm Karma Expense.
            LoadBoolFromRegistry(ref _blnConfirmKarmaExpense, "confirmkarmaexpense");
            LoadBoolFromRegistry(ref _blnHideItemsOverAvailLimit, "hideitemsoveravaillimit");
            LoadBoolFromRegistry(ref _blnAllowHoverIncrement, "allowhoverincrement");
            LoadBoolFromRegistry(ref _blnSwitchTabsOnHoverScroll, "switchtabsonhoverscroll");
            LoadBoolFromRegistry(ref _blnSearchInCategoryOnly, "searchincategoryonly");
            // Whether dice rolling is allowed for Skills.
            LoadBoolFromRegistry(ref _blnAllowSkillDiceRolling, "allowskilldicerolling");

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
            LoadStringFromRegistry(ref _strPdfParameters, "pdfparameters");

            // PDF application path.
            LoadStringFromRegistry(ref _strPdfAppPath, "pdfapppath");

            // Folder path to check for characters.
            LoadStringFromRegistry(ref _strCharacterRosterPath, "characterrosterpath");

            // Most recent image folder location used.
            LoadStringFromRegistry(ref _strImageFolder, "recentimagefolder");

            // Which Plugins are enabled.
            LoadBoolFromRegistry(ref _blnPluginsEnabled, "pluginsenabled");

            try
            {
                string strPluginsJson = string.Empty;
                LoadStringFromRegistry(ref strPluginsJson, "plugins");
                if (!string.IsNullOrEmpty(strPluginsJson))
                {
                    ConcurrentDictionary<string, bool> dicTemp
                        = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<string, bool>>(strPluginsJson);
                    if (dicTemp != null)
                    {
                        Interlocked.Exchange(ref s_dicPluginsEnabled, dicTemp);
                    }
                }
            }
            catch (Exception e)
            {
                e = e.Demystify();
                Trace.TraceError(e.Message, e);
#if DEBUG
                throw;
                /*
                *#else
                *                string msg = "Error while loading PluginOptions from registry: " + Environment.NewLine;
                *                msg += e.Message;
                *                Program.ShowScrollableMessageBox(msg);
                */
#endif
            }

            // Prefer Nightly Updates.
            LoadBoolFromRegistry(ref _blnPreferNightlyUpdates, "prefernightlybuilds");

            LoadBoolFromRegistry(ref _blnCustomDateTimeFormats, "usecustomdatetime");
            LoadStringFromRegistry(ref _strCustomDateFormat, "customdateformat");
            LoadStringFromRegistry(ref _strCustomTimeFormat, "customtimeformat");

            // Level of compression to use for .chum5lz files
            try
            {
                string strTemp = DefaultChum5lzCompressionLevel.ToString();
                LoadStringFromRegistry(ref strTemp, "chum5lzcompressionlevel");
                _eChum5lzCompressionLevel = (LzmaHelper.ChummerCompressionPreset)Enum.Parse(typeof(LzmaHelper.ChummerCompressionPreset), strTemp);
            }
            catch (Exception e)
            {
                Log.Warn(e);
                _eChum5lzCompressionLevel = DefaultChum5lzCompressionLevel;
            }

            // The quality at which images should be saved. int.MaxValue saves as Png, everything else saves as Jpeg, negative values save as Jpeg with automatic quality
            if (!LoadInt32FromRegistry(ref _intSavedImageQuality, "savedimagequality")
                // In order to not throw off veteran users, PNG is the default for them instead of Jpeg with automatic compression
                && !blnFirstEverLaunch)
                _intSavedImageQuality = int.MaxValue;

            // Retrieve CustomDataDirectoryInfo objects from registry
            using (RegistryKey objCustomDataDirectoryKey = s_ObjBaseChummerKey.OpenSubKey("CustomDataDirectory"))
            {
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
                            if (string.IsNullOrEmpty(strPath) || !Directory.Exists(strPath))
                                continue;
                            CustomDataDirectoryInfo objCustomDataDirectory
                                = new CustomDataDirectoryInfo(strDirectoryName, strPath);
                            if (objCustomDataDirectory.XmlException != default)
                            {
                                Program.ShowScrollableMessageBox(
                                    string.Format(CultureInfo, LanguageManager.GetString("Message_FailedLoad"),
                                        objCustomDataDirectory.XmlException.Message),
                                    string.Format(CultureInfo,
                                        LanguageManager.GetString("MessageTitle_FailedLoad") +
                                        LanguageManager.GetString("String_Space") + objCustomDataDirectory.Name +
                                        Path.DirectorySeparatorChar + "manifest.xml"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }

                            if (s_SetCustomDataDirectoryInfos.Contains(objCustomDataDirectory))
                            {
                                CustomDataDirectoryInfo objExistingInfo =
                                    s_SetCustomDataDirectoryInfos.FirstOrDefault(x => x.Equals(objCustomDataDirectory));
                                if (objExistingInfo != null)
                                {
                                    if (objCustomDataDirectory.HasManifest)
                                    {
                                        if (objExistingInfo.HasManifest)
                                        {
                                            Program.ShowScrollableMessageBox(
                                                string.Format(
                                                    GlobalSettings.CultureInfo,
                                                    LanguageManager.GetString("Message_Duplicate_CustomDataDirectory"),
                                                    objExistingInfo.Name, objCustomDataDirectory.Name),
                                                LanguageManager.GetString("MessageTitle_Duplicate_CustomDataDirectory"),
                                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            continue;
                                        }

                                        s_SetCustomDataDirectoryInfos.Remove(objExistingInfo);
                                        do
                                        {
                                            objExistingInfo.RandomizeGuid();
                                        } while (objExistingInfo.Equals(objCustomDataDirectory)
                                                 || s_SetCustomDataDirectoryInfos.Contains(objExistingInfo));

                                        s_SetCustomDataDirectoryInfos.Add(objExistingInfo);
                                    }
                                    else
                                    {
                                        do
                                        {
                                            objCustomDataDirectory.RandomizeGuid();
                                        } while (s_SetCustomDataDirectoryInfos.Contains(objCustomDataDirectory));
                                    }
                                }
                            }

                            s_SetCustomDataDirectoryInfos.Add(objCustomDataDirectory);
                        }
                    }
                }
            }

            // Add in default customdata directory's paths
            string strCustomDataRootPath = Utils.GetCustomDataFolderPath;
            if (Directory.Exists(strCustomDataRootPath))
            {
                foreach (string strLoopDirectoryPath in Directory.EnumerateDirectories(strCustomDataRootPath))
                {
                    // Only add directories for which we don't already have entries loaded from registry
                    if (s_SetCustomDataDirectoryInfos.Any(x => x.DirectoryPath == strLoopDirectoryPath))
                        continue;
                    CustomDataDirectoryInfo objCustomDataDirectory
                        = new CustomDataDirectoryInfo(Path.GetFileName(strLoopDirectoryPath), strLoopDirectoryPath);
                    if (objCustomDataDirectory.XmlException != default)
                    {
                        Program.ShowScrollableMessageBox(
                            string.Format(CultureInfo, LanguageManager.GetString("Message_FailedLoad"),
                                objCustomDataDirectory.XmlException.Message),
                            string.Format(CultureInfo,
                                LanguageManager.GetString("MessageTitle_FailedLoad") +
                                LanguageManager.GetString("String_Space") + objCustomDataDirectory.Name +
                                Path.DirectorySeparatorChar + "manifest.xml"), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    if (s_SetCustomDataDirectoryInfos.Contains(objCustomDataDirectory))
                    {
                        CustomDataDirectoryInfo objExistingInfo =
                            s_SetCustomDataDirectoryInfos.FirstOrDefault(x => x.Equals(objCustomDataDirectory));
                        if (objExistingInfo != null)
                        {
                            if (objCustomDataDirectory.HasManifest)
                            {
                                if (objExistingInfo.HasManifest)
                                {
                                    Program.ShowScrollableMessageBox(
                                        string.Format(
                                            GlobalSettings.CultureInfo,
                                            LanguageManager.GetString("Message_Duplicate_CustomDataDirectory"),
                                            objExistingInfo.Name, objCustomDataDirectory.Name),
                                        LanguageManager.GetString("MessageTitle_Duplicate_CustomDataDirectory"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    continue;
                                }

                                s_SetCustomDataDirectoryInfos.Remove(objExistingInfo);
                                do
                                {
                                    objExistingInfo.RandomizeGuid();
                                } while (objExistingInfo.Equals(objCustomDataDirectory)
                                         || s_SetCustomDataDirectoryInfos.Contains(objExistingInfo));

                                s_SetCustomDataDirectoryInfos.Add(objExistingInfo);
                            }
                            else
                            {
                                do
                                {
                                    objCustomDataDirectory.RandomizeGuid();
                                } while (s_SetCustomDataDirectoryInfos.Contains(objCustomDataDirectory));
                            }
                        }
                    }

                    s_SetCustomDataDirectoryInfos.Add(objCustomDataDirectory);
                }
            }

            XmlManager.RebuildDataDirectoryInfo(s_SetCustomDataDirectoryInfos);

            for (int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = s_ObjBaseChummerKey.GetValue("stickymru" + i.ToString(InvariantCultureInfo));
                if (objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if (File.Exists(strFileName) && !s_LstFavoriteCharacters.Contains(strFileName))
                        s_LstFavoriteCharacters.Add(strFileName);
                }
            }

            s_LstFavoriteCharacters.Sort();
            for (int i = 1; i <= MaxMruSize; ++i)
            {
                if (i <= s_LstFavoriteCharacters.Count)
                    s_ObjBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo), s_LstFavoriteCharacters[i - 1]);
                else
                    s_ObjBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
            }
            s_LstFavoriteCharacters.CollectionChangedAsync += LstFavoritedCharactersOnCollectionChanged;

            for (int i = 1; i <= MaxMruSize; i++)
            {
                object objLoopValue = s_ObjBaseChummerKey.GetValue("mru" + i.ToString(InvariantCultureInfo));
                if (objLoopValue != null)
                {
                    string strFileName = objLoopValue.ToString();
                    if (File.Exists(strFileName) && !s_LstMostRecentlyUsedCharacters.Contains(strFileName))
                        s_LstMostRecentlyUsedCharacters.Add(strFileName);
                }
            }

            s_LstMostRecentlyUsedCharacters.CollectionChangedAsync += LstMostRecentlyUsedCharactersOnCollectionChanged;

            if (blnFirstEverLaunch)
                ShowCharacterCustomDataWarning = false;
            else
            {
                bool blnTemp = false;
                LoadBoolFromRegistry(ref blnTemp, "charactercustomdatawarningshown");
                ShowCharacterCustomDataWarning = !blnTemp;
            }
        }

        #endregion Constructor

        #region Methods

        public static async Task SaveOptionsToRegistry(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                RegistryKey objRegistry = s_ObjBaseChummerKey ?? throw new InvalidOperationException(nameof(Registry));
                objRegistry.SetValue("autoupdate", AutomaticUpdate.ToString(InvariantCultureInfo));
                objRegistry.SetValue("livecustomdata", LiveCustomData.ToString(InvariantCultureInfo));
                objRegistry.SetValue("liveupdatecleancharacterfiles",
                                     LiveUpdateCleanCharacterFiles.ToString(InvariantCultureInfo));
                objRegistry.SetValue("dpiscalingmethod", DpiScalingMethodSetting.ToString());
                objRegistry.SetValue("uselogging", UseLogging.ToString(InvariantCultureInfo));
                objRegistry.SetValue("useloggingApplicationInsights", UseLoggingApplicationInsights.ToString());
                objRegistry.SetValue("useloggingApplicationInsightsResetCounter", UseLoggingResetCounter);
                objRegistry.SetValue("colormode", ColorModeSetting.ToString());
                objRegistry.SetValue("defaulthasnotescolor", DefaultHasNotesColor.ToArgb().ToString(InvariantCultureInfo));
                objRegistry.SetValue("language", Language);
                objRegistry.SetValue("startupfullscreen", StartupFullscreen.ToString(InvariantCultureInfo));
                objRegistry.SetValue("singlediceroller", SingleDiceRoller.ToString(InvariantCultureInfo));
                objRegistry.SetValue("defaultsheet", DefaultCharacterSheet);
                objRegistry.SetValue("datesincludetime", DatesIncludeTime.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printtofilefirst", PrintToFileFirst.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printzeroratingskills", PrintSkillsWithZeroRating.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printexpenses", PrintExpenses.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printfreeexpenses", PrintFreeExpenses.ToString(InvariantCultureInfo));
                objRegistry.SetValue("printnotes", PrintNotes.ToString(InvariantCultureInfo));
                objRegistry.SetValue("insertpdfnotesifavailable", InsertPdfNotesIfAvailable.ToString(InvariantCultureInfo));
                objRegistry.SetValue("emulatedbrowserversion",
                                     EmulatedBrowserVersion.ToString(InvariantCultureInfo));
                objRegistry.SetValue("pdfapppath", PdfAppPath);
                objRegistry.SetValue("pdfparameters", PdfParameters);
                objRegistry.SetValue("lifemodule", LifeModuleEnabled.ToString(InvariantCultureInfo));
                objRegistry.SetValue("prefernightlybuilds", PreferNightlyBuilds.ToString(InvariantCultureInfo));
                objRegistry.SetValue("characterrosterpath", CharacterRosterPath);
                objRegistry.SetValue("hidemasterindex", HideMasterIndex.ToString(InvariantCultureInfo));
                objRegistry.SetValue("hidecharacterroster", HideCharacterRoster.ToString(InvariantCultureInfo));
                objRegistry.SetValue("createbackuponcareer", CreateBackupOnCareer.ToString(InvariantCultureInfo));
                objRegistry.SetValue("confirmdelete", ConfirmDelete.ToString(InvariantCultureInfo));
                objRegistry.SetValue("confirmkarmaexpense", ConfirmKarmaExpense.ToString(InvariantCultureInfo));
                objRegistry.SetValue("hideitemsoveravaillimit",
                                     HideItemsOverAvailLimit.ToString(InvariantCultureInfo));
                objRegistry.SetValue("allowhoverincrement", AllowHoverIncrement.ToString(InvariantCultureInfo));
                objRegistry.SetValue("switchtabsonhoverscroll", SwitchTabsOnHoverScroll.ToString(InvariantCultureInfo));
                objRegistry.SetValue("searchincategoryonly", SearchInCategoryOnly.ToString(InvariantCultureInfo));
                objRegistry.SetValue("allowskilldicerolling", AllowSkillDiceRolling.ToString(InvariantCultureInfo));
                objRegistry.SetValue("pluginsenabled", PluginsEnabled.ToString(InvariantCultureInfo));
                objRegistry.SetValue("alloweastereggs", AllowEasterEggs.ToString(InvariantCultureInfo));
                objRegistry.DeleteValue("defaultcharacteroption", false); // For 5.214.x Nightly users
                objRegistry.SetValue("defaultcharactersetting", DefaultCharacterSetting);
                objRegistry.SetValue("defaultmasterindexsetting", DefaultMasterIndexSetting);
                objRegistry.SetValue("usecustomdatetime", CustomDateTimeFormats.ToString(InvariantCultureInfo));
                if (CustomDateFormat != null)
                    objRegistry.SetValue("customdateformat", CustomDateFormat);
                if (CustomTimeFormat != null)
                    objRegistry.SetValue("customtimeformat", CustomTimeFormat);
                objRegistry.SetValue("chum5lzcompressionlevel", Chum5lzCompressionLevel.ToString());
                objRegistry.SetValue("savedimagequality", SavedImageQuality.ToString(InvariantCultureInfo));

                //Save the Plugins-Dictionary
                objRegistry.SetValue("plugins", Newtonsoft.Json.JsonConvert.SerializeObject(PluginsEnabledDic));

                token.ThrowIfCancellationRequested();
                // Save the SourcebookInfo.
                using (RegistryKey objSourceRegistry = objRegistry.CreateSubKey("Sourcebook", true))
                {
                    if (objSourceRegistry != null)
                    {
                        foreach (string strCustomSourcebookKey in s_DicCustomSourcebookCodes)
                            objSourceRegistry.DeleteValue(strCustomSourcebookKey, false);

                        IReadOnlyDictionary<string, SourcebookInfo> dicSourcebookInfos = await GetSourcebookInfosAsync(token).ConfigureAwait(false);
                        foreach (KeyValuePair<string, SourcebookInfo> kvpSourcebookInfo in dicSourcebookInfos)
                        {
                            SourcebookInfo objSourcebookInfo = kvpSourcebookInfo.Value; // Set up this way to avoid race condition in underlying SourcebookInfos dictionary
                            objSourceRegistry.SetValue(objSourcebookInfo.Code,
                                objSourcebookInfo.Path + '|'
                                                       + objSourcebookInfo.Offset.ToString(
                                                           InvariantCultureInfo));
                        }
                    }
                }

                // Save the Custom Data Directory Info.
                objRegistry.DeleteSubKeyTree("CustomDataDirectory", false);
                using (RegistryKey objCustomDataDirectoryRegistry = objRegistry.CreateSubKey("CustomDataDirectory", true))
                {
                    if (objCustomDataDirectoryRegistry != null)
                    {
                        foreach (CustomDataDirectoryInfo objCustomDataDirectory in CustomDataDirectoryInfos)
                        {
                            token.ThrowIfCancellationRequested();
                            using (RegistryKey objLoopKey =
                                   objCustomDataDirectoryRegistry.CreateSubKey(objCustomDataDirectory.Name, true))
                            {
                                objLoopKey?.SetValue("Path",
                                                     objCustomDataDirectory.DirectoryPath.Replace(
                                                         Utils.GetStartupPath, "$CHUMMER"));
                            }
                        }
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (ArgumentNullException e) when (e.ParamName == nameof(Registry))
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Whether to create backups of characters before moving them to career mode. If true, a separate save file is created before marking the current character as created.
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
        /// Whether the Master Index should be shown. If true, prevents the roster from being removed or hidden.
        /// </summary>
        public static bool HideMasterIndex
        {
            get => _blnHideMasterIndex;
            set => _blnHideMasterIndex = value;
        }

        /// <summary>
        /// Whether the Character Roster should be shown. If true, prevents the roster from being removed or hidden.
        /// </summary>
        public static bool HideCharacterRoster
        {
            get => _blnHideCharacterRoster;
            set => _blnHideCharacterRoster = value;
        }

        /// <summary>
        /// DPI Scaling method to use
        /// </summary>
        public static DpiScalingMethod DpiScalingMethodSetting
        {
            get => _eDpiScalingMethod;
            set => _eDpiScalingMethod = value;
        }

        /// <summary>
        /// Whether Automatic Updates are enabled.
        /// </summary>
        public static bool AutomaticUpdate
        {
            get => _blnAutomaticUpdate;
            set => _blnAutomaticUpdate = value;
        }

        /// <summary>
        /// Whether live updates from the customdata directory are allowed.
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
        /// Whether confirmation messages are shown when deleting an object.
        /// </summary>
        public static bool ConfirmDelete
        {
            get => _blnConfirmDelete;
            set => _blnConfirmDelete = value;
        }

        /// <summary>
        /// Whether confirmation messages are shown for Karma Expenses.
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
        /// Whether numeric updowns can increment values of numericupdown controls by hovering over the control.
        /// </summary>
        public static bool AllowHoverIncrement
        {
            get => _blnAllowHoverIncrement;
            set => _blnAllowHoverIncrement = value;
        }

        /// <summary>
        /// Whether scrolling the mouse wheel while hovering over tab page labels switches tabs
        /// </summary>
        public static bool SwitchTabsOnHoverScroll
        {
            get => _blnSwitchTabsOnHoverScroll;
            set => _blnSwitchTabsOnHoverScroll = value;
        }

        /// <summary>
        /// Whether searching in a selection form will limit itself to the current Category that's selected.
        /// </summary>
        public static bool SearchInCategoryOnly
        {
            get => _blnSearchInCategoryOnly;
            set => _blnSearchInCategoryOnly = value;
        }

        public static NumericUpDownEx.InterceptMouseWheelMode InterceptMode => AllowHoverIncrement
            ? NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver
            : NumericUpDownEx.InterceptMouseWheelMode.WhenFocus;

        /// <summary>
        /// Whether dice rolling is allowed for Skills.
        /// </summary>
        public static bool AllowSkillDiceRolling
        {
            get => _blnAllowSkillDiceRolling;
            set => _blnAllowSkillDiceRolling = value;
        }

        /// <summary>
        /// Whether the app should use logging.
        /// </summary>
        public static bool UseLogging
        {
            get => _blnUseLogging;
            set
            {
                if (_blnUseLogging == value)
                    return;
                _blnUseLogging = value;
                // Sets up logging if the option is changed during runtime
                if (value)
                {
                    if (!LogManager.IsLoggingEnabled())
                        LogManager.ResumeLogging();
                }
                else if (LogManager.IsLoggingEnabled())
                    LogManager.SuspendLogging();
            }
        }

        /// <summary>
        /// Should actually the set LoggingLevel be used or only a more conservative one
        /// </summary>
        public static int UseLoggingResetCounter
        {
            get => _intResetLogging;
            set => _intResetLogging = value;
        }

        /// <summary>
        /// What Logging Level are we "allowed" to use by the user. The actual used Level is the UseLoggingApplicationInsights and depends on
        /// nightly/stable and ResetLoggingCounter
        /// </summary>
        public static UseAILogging UseLoggingApplicationInsightsPreference
        {
            get => _eUseLoggingApplicationInsights;
            set
            {
                bool blnNewDisableTelemetry = value < UseAILogging.OnlyMetric;
                bool blnOldDisableTelemetry = InterlockedExtensions.Exchange(ref _eUseLoggingApplicationInsights, value)
                                              < UseAILogging.OnlyMetric;
                if (blnOldDisableTelemetry != blnNewDisableTelemetry && Program.ChummerTelemetryClient.IsValueCreated)
                {
                    // Sets up logging if the option is changed during runtime
                    TelemetryConfiguration objConfiguration = Program.ActiveTelemetryConfiguration;
                    if (objConfiguration != null)
                        objConfiguration.DisableTelemetry = blnNewDisableTelemetry;
                }
            }
        }

        /// <summary>
        /// Whether the app should use logging.
        /// </summary>
        public static UseAILogging UseLoggingApplicationInsights
        {
            get
            {
                if (UseLoggingApplicationInsightsPreference == UseAILogging.OnlyLocal)
                    return UseAILogging.OnlyLocal;

                if (UseLoggingResetCounter > 0)
                    return UseLoggingApplicationInsightsPreference;

                if (Utils.IsMilestoneVersion
                    //stable builds should not log more than metrics
                    && UseLoggingApplicationInsightsPreference > UseAILogging.OnlyMetric)
                    return UseAILogging.OnlyMetric;

                return UseLoggingApplicationInsightsPreference;
            }
            set => UseLoggingApplicationInsightsPreference = value;
        }

        /// <summary>
        /// Whether the program should be forced to use Light/Dark mode or to obey default color schemes automatically
        /// </summary>
        public static ColorMode ColorModeSetting => _eColorMode;

        public static Task SetColorModeSettingAsync(ColorMode value, CancellationToken token = default)
        {
            if (InterlockedExtensions.Exchange(ref _eColorMode, value) == value)
                return Task.CompletedTask;
            switch (value)
            {
                case ColorMode.Automatic:
                    return ColorManager.AutoApplyLightDarkModeAsync(token);

                case ColorMode.Light:
                    ColorManager.DisableAutoTimer();
                    return ColorManager.SetIsLightModeAsync(true, token);

                case ColorMode.Dark:
                    ColorManager.DisableAutoTimer();
                    return ColorManager.SetIsLightModeAsync(false, token);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Default (light mode) color to use for if/when an item has notes.
        /// If you are requesting this directly, you are probably doing something wrong. What you want is ColorManager.HasNotesColor instead.
        /// </summary>
        public static Color DefaultHasNotesColor
        {
            get => _objDefaultHasNotesColor;
            set => _objDefaultHasNotesColor = value;
        }

        /// <summary>
        /// Whether dates should include the time.
        /// </summary>
        public static bool DatesIncludeTime
        {
            get => _blnDatesIncludeTime;
            set => _blnDatesIncludeTime = value;
        }

        /// <summary>
        /// Whether printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
        /// </summary>
        public static bool PrintToFileFirst
        {
            get => _blnPrintToFileFirst;
            set => _blnPrintToFileFirst = value;
        }

        /// <summary>
        /// Whether all Active Skills with a total score higher than 0 should be printed.
        /// </summary>
        public static bool PrintSkillsWithZeroRating
        {
            get => _blnPrintSkillsWithZeroRating;
            set => _blnPrintSkillsWithZeroRating = value;
        }

        /// <summary>
        /// Whether the Karma and Nuyen Expenses should be printed on the character sheet.
        /// </summary>
        public static bool PrintExpenses
        {
            get => _blnPrintExpenses;
            set => _blnPrintExpenses = value;
        }

        /// <summary>
        /// Whether the Karma and Nuyen Expenses that have a cost of 0 should be printed on the character sheet.
        /// </summary>
        public static bool PrintFreeExpenses
        {
            get => _blnPrintFreeExpenses;
            set => _blnPrintFreeExpenses = value;
        }

        /// <summary>
        /// Whether Notes should be printed.
        /// </summary>
        public static bool PrintNotes
        {
            get => _blnPrintNotes;
            set => _blnPrintNotes = value;
        }

        /// <summary>
        /// Whether to insert scraped text from PDFs into the Notes fields of newly added items.
        /// </summary>
        public static bool InsertPdfNotesIfAvailable
        {
            get => _blnInsertPdfNotesIfAvailable;
            set => _blnInsertPdfNotesIfAvailable = value;
        }

        /// <summary>
        /// Which version of the Internet Explorer's rendering engine will be emulated for rendering the character view. Defaults to 11
        /// </summary>
        public static int EmulatedBrowserVersion
        {
            get => _intEmulatedBrowserVersion;
            set
            {
                if (Interlocked.Exchange(ref _intEmulatedBrowserVersion, value) != value)
                    Utils.SetupWebBrowserRegistryKeys();
            }
        }

        /// <summary>
        /// Chummer's UI Language.
        /// </summary>
        public static string Language
        {
            get => _strLanguage;
            set
            {
                if (Interlocked.Exchange(ref _strLanguage, value) == value)
                    return;
                try
                {
                    _objLanguageCultureInfo = CultureInfo.GetCultureInfo(value);
                }
                catch (CultureNotFoundException)
                {
                    _objLanguageCultureInfo = SystemCultureInfo;
                }
                // Set default cultures based on the currently set language
                CultureInfo.DefaultThreadCurrentCulture = _objLanguageCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = _objLanguageCultureInfo;
                ChummerMainForm frmMain = Program.MainForm;
                if (frmMain == null)
                    return;
                try
                {
                    frmMain.TranslateWinForm();
                    IReadOnlyCollection<Form> lstToProcess = frmMain.OpenCharacterEditorForms;
                    if (lstToProcess != null)
                    {
                        foreach (Form frmLoop in lstToProcess)
                        {
                            frmLoop.TranslateWinForm();
                        }
                    }

                    lstToProcess = frmMain.OpenCharacterSheetViewers;
                    if (lstToProcess != null)
                    {
                        foreach (Form frmLoop in lstToProcess)
                        {
                            frmLoop.TranslateWinForm();
                        }
                    }

                    lstToProcess = frmMain.OpenCharacterExportForms;
                    if (lstToProcess != null)
                    {
                        foreach (Form frmLoop in lstToProcess)
                        {
                            frmLoop.TranslateWinForm();
                        }
                    }

                    frmMain.PrintMultipleCharactersForm?.TranslateWinForm();
                    frmMain.CharacterRoster?.TranslateWinForm();
                    frmMain.MasterIndex?.TranslateWinForm();
                    frmMain.RefreshAllTabTitles();
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        public static async Task SetLanguageAsync(string value, CancellationToken token = default)
        {
            if (Interlocked.Exchange(ref _strLanguage, value) == value)
                return;
            try
            {
                _objLanguageCultureInfo = CultureInfo.GetCultureInfo(value);
            }
            catch (CultureNotFoundException)
            {
                _objLanguageCultureInfo = SystemCultureInfo;
            }
            // Set default cultures based on the currently set language
            CultureInfo.DefaultThreadCurrentCulture = _objLanguageCultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = _objLanguageCultureInfo;
            ChummerMainForm frmMain = Program.MainForm;
            if (frmMain != null)
            {
                await frmMain.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                IReadOnlyCollection<Form> lstToProcess = frmMain.OpenCharacterEditorForms;
                if (lstToProcess != null)
                {
                    foreach (Form frmLoop in lstToProcess)
                    {
                        await frmLoop.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                    }
                }
                lstToProcess = frmMain.OpenCharacterSheetViewers;
                if (lstToProcess != null)
                {
                    foreach (Form frmLoop in lstToProcess)
                    {
                        await frmLoop.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                    }
                }
                lstToProcess = frmMain.OpenCharacterExportForms;
                if (lstToProcess != null)
                {
                    foreach (Form frmLoop in lstToProcess)
                    {
                        await frmLoop.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                    }
                }
                Form frmToProcess = frmMain.PrintMultipleCharactersForm;
                if (frmToProcess != null)
                    await frmToProcess.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                frmToProcess = frmMain.CharacterRoster;
                if (frmToProcess != null)
                    await frmToProcess.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                frmToProcess = frmMain.MasterIndex;
                if (frmToProcess != null)
                    await frmToProcess.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                await frmMain.RefreshAllTabTitlesAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the application should start in fullscreen mode.
        /// </summary>
        public static bool StartupFullscreen
        {
            get => _blnStartupFullscreen;
            set => _blnStartupFullscreen = value;
        }

        /// <summary>
        /// Whether only a single instance of the Dice Roller should be allowed.
        /// </summary>
        public static bool SingleDiceRoller
        {
            get => _blnSingleDiceRoller;
            set => _blnSingleDiceRoller = value;
        }

        /// <summary>
        /// CultureInfo for number localization.
        /// </summary>
        public static CultureInfo CultureInfo => _objLanguageCultureInfo;

        /// <summary>
        /// Invariant CultureInfo for saving and loading of numbers.
        /// </summary>
        public static CultureInfo InvariantCultureInfo => CultureInfo.InvariantCulture;

        /// <summary>
        /// CultureInfo of the user's current system.
        /// </summary>
        public static CultureInfo SystemCultureInfo => CultureInfo.CurrentCulture;

        private static readonly XmlDocument s_xmlClipboard = new XmlDocument { XmlResolver = null };
        private static readonly AsyncFriendlyReaderWriterLock _objClipboardLocker = new AsyncFriendlyReaderWriterLock();

        private static ClipboardContentType _eClipboardContentType;

        /// <summary>
        /// XmlReaderSettings that should be used when reading almost Xml readable.
        /// </summary>
        public static XmlReaderSettings SafeXmlReaderSettings { get; } = new XmlReaderSettings { XmlResolver = null, IgnoreComments = true, IgnoreWhitespace = true };

        /// <summary>
        /// XmlReaderSettings that should only be used if invalid characters are found.
        /// </summary>
        public static XmlReaderSettings UnSafeXmlReaderSettings { get; } = new XmlReaderSettings { XmlResolver = null, IgnoreComments = true, IgnoreWhitespace = true, CheckCharacters = false };

        /// <summary>
        /// XmlReaderSettings that should be used when reading almost Xml readable.
        /// </summary>
        public static XmlReaderSettings SafeXmlReaderAsyncSettings { get; } = new XmlReaderSettings { XmlResolver = null, IgnoreComments = true, IgnoreWhitespace = true, Async = true };

        /// <summary>
        /// XmlReaderSettings that should only be used if invalid characters are found.
        /// </summary>
        public static XmlReaderSettings UnSafeXmlReaderAsyncSettings { get; } = new XmlReaderSettings { XmlResolver = null, IgnoreComments = true, IgnoreWhitespace = true, CheckCharacters = false, Async = true };

        /// <summary>
        /// Lock the clipboard for reading.
        /// </summary>
        public static IDisposable EnterClipboardReadLock(CancellationToken token = default) =>
            _objClipboardLocker.EnterReadLock(token);

        /// <summary>
        /// Lock the clipboard for reading.
        /// </summary>
        public static Task<IAsyncDisposable> EnterClipboardReadLockAsync(CancellationToken token = default) =>
            _objClipboardLocker.EnterReadLockAsync(token);

        /// <summary>
        /// Lock the clipboard for upgradeable reading.
        /// </summary>
        public static IDisposable EnterClipboardUpgradeableReadLock(CancellationToken token = default) =>
            _objClipboardLocker.EnterUpgradeableReadLock(token);

        /// <summary>
        /// Lock the clipboard for upgradeable reading.
        /// </summary>
        public static Task<IAsyncDisposable> EnterClipboardUpgradeableReadLockAsync(CancellationToken token = default) =>
            _objClipboardLocker.EnterUpgradeableReadLockAsync(token);

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static XmlDocument Clipboard
        {
            get
            {
                using (_objClipboardLocker.EnterReadLock())
                    return s_xmlClipboard;
            }
        }

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static void SetClipboard(XmlDocument value, ClipboardContentType eType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strNewOuterXml = value.OuterXmlViaPool();
            using (_objClipboardLocker.EnterReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                if (eType == _eClipboardContentType && s_xmlClipboard.OuterXmlViaPool() == strNewOuterXml)
                    return;
            }
            using (_objClipboardLocker.EnterUpgradeableReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                if (eType == _eClipboardContentType && s_xmlClipboard.OuterXmlViaPool() == strNewOuterXml)
                    return;

                using (_objClipboardLocker.EnterWriteLock(token))
                {
                    token.ThrowIfCancellationRequested();
                    _eClipboardContentType = eType;
                    s_xmlClipboard.RemoveAll();
                    s_xmlClipboard.ImportNode(value, true);

                    if (ClipboardChangedAsync != null)
                        Utils.SafelyRunSynchronously(() => ClipboardChangedAsync.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)), token), token);
                    ClipboardChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)));
                }
            }
        }

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static async Task<XmlDocument> GetClipboardAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objClipboardLocker.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return s_xmlClipboard;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clipboard.
        /// </summary>
        public static async Task SetClipboardAsync(XmlNode value, ClipboardContentType eType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strNewOuterXml = value.OuterXmlViaPool();
            IAsyncDisposable objLocker = await _objClipboardLocker.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (eType == _eClipboardContentType && s_xmlClipboard.OuterXmlViaPool() == strNewOuterXml)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await _objClipboardLocker.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (eType == _eClipboardContentType && s_xmlClipboard.OuterXmlViaPool() == strNewOuterXml)
                    return;

                IAsyncDisposable objLocker2 = await _objClipboardLocker.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _eClipboardContentType = eType;
                    s_xmlClipboard.RemoveAll();
                    s_xmlClipboard.ImportNode(value, true);

                    if (ClipboardChangedAsync != null)
                        await ClipboardChangedAsync.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)), token).ConfigureAwait(false);
                    ClipboardChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Clipboard)));
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public static ClipboardContentType ClipboardContentType
        {
            get
            {
                using (_objClipboardLocker.EnterReadLock())
                    return _eClipboardContentType;
            }
        }

        /// <summary>
        /// Type of data that is currently stored in the clipboard.
        /// </summary>
        public static async Task<ClipboardContentType> GetClipboardContentTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objClipboardLocker.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eClipboardContentType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Default character sheet to use when printing.
        /// </summary>
        public static string DefaultCharacterSheet
        {
            get => _strDefaultCharacterSheet;
            set => _strDefaultCharacterSheet = value;
        }

        /// <summary>
        /// Default setting to select when creating a new character
        /// </summary>
        public static string DefaultCharacterSetting
        {
            get => _strDefaultCharacterSetting;
            set => _strDefaultCharacterSetting = value;
        }

        /// <summary>
        /// Default setting to select when opening the Master Index for the first time
        /// </summary>
        public static string DefaultMasterIndexSetting
        {
            get => _strDefaultMasterIndexSetting;
            set => _strDefaultMasterIndexSetting = value;
        }

        /// <summary>
        /// Path to the user's PDF application.
        /// </summary>
        public static string PdfAppPath
        {
            get => _strPdfAppPath;
            set => _strPdfAppPath = value;
        }

        /// <summary>
        /// Parameter style to use when opening a PDF with the PDF application specified in PdfAppPath
        /// </summary>
        public static string PdfParameters
        {
            get => _strPdfParameters;
            set => _strPdfParameters = value;
        }

        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public static IReadOnlyDictionary<string, SourcebookInfo> SourcebookInfos
        {
            get
            {
                // We need to generate s_DicSourcebookInfos outside of the constructor to avoid initialization cycles
                while (s_intSourcebookInfosLoadingStatus != 2)
                {
                    LoadSourcebookInfos();
                    if (s_intSourcebookInfosLoadingStatus != 2)
                        Utils.SafeSleep();
                }
                return s_DicSourcebookInfos;
            }
        }

        /// <summary>
        /// List of SourcebookInfo.
        /// </summary>
        public static async Task<IReadOnlyDictionary<string, SourcebookInfo>> GetSourcebookInfosAsync(CancellationToken token = default)
        {
            // We need to generate s_DicSourcebookInfos outside of the constructor to avoid initialization cycles
            while (s_intSourcebookInfosLoadingStatus != 2)
            {
                await LoadSourcebookInfosAsync(token).ConfigureAwait(false);
                if (s_intSourcebookInfosLoadingStatus != 2)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            }
            return s_DicSourcebookInfos;
        }

        public static void SetSourcebookInfos(IReadOnlyDictionary<string, SourcebookInfo> dicNewValues, bool blnDisposeOldInfos = true, CancellationToken token = default)
        {
            while (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 2) != 2)
                Utils.SafeSleep(token);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnDisposeOldInfos)
                {
                    List<SourcebookInfo> lstInfos = s_DicSourcebookInfos.GetValuesToListSafe();
                    s_DicSourcebookInfos.Clear();
                    foreach (SourcebookInfo objInfo in lstInfos)
                        objInfo.Dispose();
                }
                else
                    s_DicSourcebookInfos.Clear();
                token.ThrowIfCancellationRequested();
                foreach (SourcebookInfo objSourcebookInfo in dicNewValues.Values)
                {
                    token.ThrowIfCancellationRequested();
                    s_DicSourcebookInfos.TryAdd(objSourcebookInfo.Code, objSourcebookInfo);
                }
            }
            finally
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
            }
        }

        public static async Task SetSourcebookInfosAsync(IReadOnlyDictionary<string, SourcebookInfo> dicNewValues, bool blnDisposeOldInfos = true, CancellationToken token = default)
        {
            while (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 2) != 2)
                await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnDisposeOldInfos)
                {
                    List<SourcebookInfo> lstInfos = s_DicSourcebookInfos.GetValuesToListSafe();
                    s_DicSourcebookInfos.Clear();
                    foreach (SourcebookInfo objInfo in lstInfos)
                        objInfo.Dispose();
                }
                else
                    s_DicSourcebookInfos.Clear();
                token.ThrowIfCancellationRequested();
                foreach (SourcebookInfo objSourcebookInfo in dicNewValues.Values)
                {
                    token.ThrowIfCancellationRequested();
                    s_DicSourcebookInfos.TryAdd(objSourcebookInfo.Code, objSourcebookInfo);
                }
            }
            finally
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
            }
        }

        private static void LoadSourcebookInfos(CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 0) != 0)
                return;
            try
            {
                List<SourcebookInfo> lstInfos = s_DicSourcebookInfos.GetValuesToListSafe();
                s_DicSourcebookInfos.Clear();
                foreach (SourcebookInfo objInfo in lstInfos)
                    objInfo.Dispose();
                foreach (XPathNavigator xmlBook in XmlManager.LoadXPath("books.xml", token: token)
                             .SelectAndCacheExpression("/chummer/books/book", token: token))
                {
                    string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                    if (string.IsNullOrEmpty(strCode))
                        continue;
                    SourcebookInfo objSource = new SourcebookInfo
                    {
                        Code = strCode
                    };

                    try
                    {
                        string strTemp = string.Empty;
                        if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                            && !string.IsNullOrEmpty(strTemp))
                        {
                            string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                            try
                            {
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
                                    string strSecondPart = strParts[1];
                                    if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                        objSource.Offset = intTmp;
                                }
                            }
                            finally
                            {
                                ArrayPool<string>.Shared.Return(strParts);
                            }
                        }
                    }
                    catch (System.Security.SecurityException)
                    {
                        //swallow this
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //swallow this
                    }

                    s_DicSourcebookInfos.TryAdd(strCode, objSource);
                }

                s_DicCustomSourcebookCodes.Clear();
                foreach (KeyValuePair<string, CharacterSettings> kvpCustomSettings in SettingsManager.LoadedCharacterSettings)
                {
                    CharacterSettings objCustomSettings = kvpCustomSettings.Value; // Set up this way to avoid race condition in underlying dictionary
                    foreach (XPathNavigator xmlBook in XmlManager.LoadXPath("books.xml",
                                     objCustomSettings.EnabledCustomDataDirectoryPaths, token: token)
                                 .SelectAndCacheExpression("/chummer/books/book", token: token))
                    {
                        string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                        if (string.IsNullOrEmpty(strCode) || s_DicSourcebookInfos.ContainsKey(strCode))
                            continue;
                        SourcebookInfo objSource = new SourcebookInfo
                        {
                            Code = strCode
                        };

                        try
                        {
                            string strTemp = string.Empty;
                            if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                                && !string.IsNullOrEmpty(strTemp))
                            {
                                string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                                try
                                {
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
                                        string strSecondPart = strParts[1];
                                        if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                            objSource.Offset = intTmp;
                                    }
                                }
                                finally
                                {
                                    ArrayPool<string>.Shared.Return(strParts);
                                }
                            }
                        }
                        catch (System.Security.SecurityException)
                        {
                            //swallow this
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //swallow this
                        }

                        s_DicSourcebookInfos.TryAdd(strCode, objSource);
                        s_DicCustomSourcebookCodes.TryAdd(strCode);
                    }
                }
            }
            catch
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 0, 1);
                throw;
            }
            finally
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
            }
        }

        private static async Task LoadSourcebookInfosAsync(CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 0) != 0)
                return;
            try
            {
                token.ThrowIfCancellationRequested();
                List<SourcebookInfo> lstInfos = s_DicSourcebookInfos.GetValuesToListSafe();
                s_DicSourcebookInfos.Clear();
                foreach (SourcebookInfo objInfo in lstInfos)
                    objInfo.Dispose();
                foreach (XPathNavigator xmlBook in (await XmlManager.LoadXPathAsync("books.xml", token: token)
                             .ConfigureAwait(false))
                         .SelectAndCacheExpression(
                             "/chummer/books/book", token: token))
                {
                    string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                    if (string.IsNullOrEmpty(strCode))
                        continue;
                    SourcebookInfo objSource = new SourcebookInfo
                    {
                        Code = strCode
                    };

                    try
                    {
                        string strTemp = string.Empty;
                        if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                            && !string.IsNullOrEmpty(strTemp))
                        {
                            string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                            try
                            {
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
                                    string strSecondPart = strParts[1];
                                    if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                        objSource.Offset = intTmp;
                                }
                            }
                            finally
                            {
                                ArrayPool<string>.Shared.Return(strParts);
                            }
                        }
                    }
                    catch (System.Security.SecurityException)
                    {
                        //swallow this
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //swallow this
                    }

                    s_DicSourcebookInfos.TryAdd(strCode, objSource);
                }

                s_DicCustomSourcebookCodes.Clear();
                foreach (KeyValuePair<string, CharacterSettings> kvpCustomSettings in (await SettingsManager
                             .GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false)))
                {
                    CharacterSettings objCustomSettings = kvpCustomSettings.Value; // Set up this way to avoid race condition in underlying dictionary
                    foreach (XPathNavigator xmlBook in (await XmlManager.LoadXPathAsync("books.xml",
                                 await objCustomSettings.GetEnabledCustomDataDirectoryPathsAsync(token)
                                     .ConfigureAwait(false), token: token).ConfigureAwait(false))
                             .SelectAndCacheExpression("/chummer/books/book", token: token))
                    {
                        string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                        if (string.IsNullOrEmpty(strCode) || s_DicSourcebookInfos.ContainsKey(strCode))
                            continue;
                        SourcebookInfo objSource = new SourcebookInfo
                        {
                            Code = strCode
                        };

                        try
                        {
                            string strTemp = string.Empty;
                            if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                                && !string.IsNullOrEmpty(strTemp))
                            {
                                string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                                try
                                {
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
                                        string strSecondPart = strParts[1];
                                        if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                            objSource.Offset = intTmp;
                                    }
                                }
                                finally
                                {
                                    ArrayPool<string>.Shared.Return(strParts);
                                }
                            }
                        }
                        catch (System.Security.SecurityException)
                        {
                            //swallow this
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //swallow this
                        }

                        s_DicSourcebookInfos.TryAdd(strCode, objSource);
                        s_DicCustomSourcebookCodes.TryAdd(strCode);
                    }
                }
            }
            catch
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 0, 1);
                throw;
            }
            finally
            {
                Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
            }
        }

        public static void ReloadCustomSourcebookInfos(CancellationToken token = default)
        {
            s_intSourcebookInfosLoadingStatus = 0;
            while (s_intSourcebookInfosLoadingStatus != 2)
            {
                if (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 0) == 0)
                {
                    try
                    {
                        foreach (string strCode in s_DicCustomSourcebookCodes)
                        {
                            if (s_DicSourcebookInfos.TryRemove(strCode, out SourcebookInfo objInfo))
                                objInfo.Dispose();
                        }

                        s_DicCustomSourcebookCodes.Clear();
                        foreach (KeyValuePair<string, CharacterSettings> kvpCustomSettings in SettingsManager.LoadedCharacterSettings)
                        {
                            CharacterSettings objCustomSettings = kvpCustomSettings.Value; // Set up this way to avoid race condition in underlying dictionary
                            foreach (XPathNavigator xmlBook in XmlManager.LoadXPath("books.xml",
                                             objCustomSettings.EnabledCustomDataDirectoryPaths, token: token)
                                         .SelectAndCacheExpression("/chummer/books/book", token: token))
                            {
                                string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)
                                    ?.Value;
                                if (string.IsNullOrEmpty(strCode) || s_DicSourcebookInfos.ContainsKey(strCode))
                                    continue;
                                SourcebookInfo objSource = new SourcebookInfo
                                {
                                    Code = strCode
                                };

                                try
                                {
                                    string strTemp = string.Empty;
                                    if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                                        && !string.IsNullOrEmpty(strTemp))
                                    {
                                        string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                                        try
                                        {
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
                                                string strSecondPart = strParts[1];
                                                if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                                    objSource.Offset = intTmp;
                                            }
                                        }
                                        finally
                                        {
                                            ArrayPool<string>.Shared.Return(strParts);
                                        }
                                    }
                                }
                                catch (System.Security.SecurityException)
                                {
                                    //swallow this
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    //swallow this
                                }

                                s_DicSourcebookInfos.TryAdd(strCode, objSource);
                                s_DicCustomSourcebookCodes.TryAdd(strCode);
                            }
                        }
                    }
                    catch
                    {
                        Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 0, 1);
                        throw;
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
                    }
                }

                if (s_intSourcebookInfosLoadingStatus != 2)
                    Utils.SafeSleep(token);
            }
        }

        public static async Task ReloadCustomSourcebookInfosAsync(CancellationToken token = default)
        {
            s_intSourcebookInfosLoadingStatus = 0;
            while (s_intSourcebookInfosLoadingStatus != 2)
            {
                if (Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 1, 0) == 0)
                {
                    try
                    {
                        foreach (string strCode in s_DicCustomSourcebookCodes)
                        {
                            if (s_DicSourcebookInfos.TryRemove(strCode, out SourcebookInfo objInfo))
                                objInfo.Dispose();
                        }

                        s_DicCustomSourcebookCodes.Clear();
                        foreach (KeyValuePair<string, CharacterSettings> kvpCustomSettings in (await SettingsManager
                                     .GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false)))
                        {
                            CharacterSettings objCustomSettings = kvpCustomSettings.Value; // Set up this way to avoid race condition in underlying dictionary
                            foreach (XPathNavigator xmlBook in (await XmlManager.LoadXPathAsync("books.xml",
                                         await objCustomSettings.GetEnabledCustomDataDirectoryPathsAsync(token)
                                             .ConfigureAwait(false), token: token).ConfigureAwait(false))
                                     .SelectAndCacheExpression("/chummer/books/book", token: token))
                            {
                                string strCode = xmlBook.SelectSingleNodeAndCacheExpression("code", token: token)
                                    ?.Value;
                                if (string.IsNullOrEmpty(strCode) || s_DicSourcebookInfos.ContainsKey(strCode))
                                    continue;
                                SourcebookInfo objSource = new SourcebookInfo
                                {
                                    Code = strCode
                                };

                                try
                                {
                                    string strTemp = string.Empty;
                                    if (LoadStringFromRegistry(ref strTemp, strCode, "Sourcebook")
                                        && !string.IsNullOrEmpty(strTemp))
                                    {
                                        string[] strParts = strTemp.SplitFixedSizePooledArray(2, '|');
                                        try
                                        {
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
                                                string strSecondPart = strParts[1];
                                                if (!string.IsNullOrEmpty(strSecondPart) && int.TryParse(strSecondPart, out int intTmp))
                                                    objSource.Offset = intTmp;
                                            }
                                        }
                                        finally
                                        {
                                            ArrayPool<string>.Shared.Return(strParts);
                                        }
                                    }
                                }
                                catch (System.Security.SecurityException)
                                {
                                    //swallow this
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    //swallow this
                                }

                                s_DicSourcebookInfos.TryAdd(strCode, objSource);
                                s_DicCustomSourcebookCodes.TryAdd(strCode);
                            }
                        }
                    }
                    catch
                    {
                        Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 0, 1);
                        throw;
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref s_intSourcebookInfosLoadingStatus, 2, 1);
                    }
                }

                if (s_intSourcebookInfosLoadingStatus != 2)
                    await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Dictionary of custom data directory infos keyed to their internal IDs.
        /// </summary>
        public static HashSet<CustomDataDirectoryInfo> CustomDataDirectoryInfos => s_SetCustomDataDirectoryInfos;

        /// <summary>
        /// Should the updater check for Release builds, or Nightly builds
        /// </summary>
        public static bool PreferNightlyBuilds
        {
            get => _blnPreferNightlyUpdates;
            set => _blnPreferNightlyUpdates = value;
        }

        /// <summary>
        /// Whether to show a warning that this Nightly build is special
        /// </summary>
        public static bool ShowCharacterCustomDataWarning
        {
            get => _blnShowCharacterCustomDataWarning;
            set
            {
                if (_blnShowCharacterCustomDataWarning == value)
                    return;
                _blnShowCharacterCustomDataWarning = value;
                using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
                {
                    if (objRegistry == null)
                        return;
                    if (value)
                    {
                        objRegistry.DeleteValue("charactercustomdatawarningshown", false);
                    }
                    else
                    {
                        objRegistry.SetValue("charactercustomdatawarningshown", bool.TrueString);
                    }
                }
            }
        }

        /// <summary>
        /// Path to the directory that Chummer should watch and from which to automatically populate its character roster.
        /// </summary>
        public static string CharacterRosterPath
        {
            get => _strCharacterRosterPath;
            set => _strCharacterRosterPath = value;
        }

        public static string PdfArguments { get; internal set; }

        /// <summary>
        /// Compression quality to use when saving images. int.MaxValue is PNG (Lossless), anything else that is positive is JPEG (Lossy),
        /// anything else that is negative is JPEG with quality set automatically based on the size of the image.
        /// </summary>
        public static int SavedImageQuality
        {
            get => _intSavedImageQuality;
            set => _intSavedImageQuality = value;
        }

        /// <summary>
        /// Converts an image to its Base64 string equivalent with compression settings specified by SavedImageQuality.
        /// </summary>
        /// <param name="objImageToSave">Image whose Base64 string should be created.</param>
        public static string ImageToBase64StringForStorage(Image objImageToSave, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return SavedImageQuality == int.MaxValue
                ? objImageToSave.ToBase64String(token: token)
                : objImageToSave.ToBase64StringAsJpeg(SavedImageQuality, token);
        }

        /// <summary>
        /// Converts an image to its Base64 string equivalent with compression settings specified by SavedImageQuality.
        /// </summary>
        /// <param name="objImageToSave">Image whose Base64 string should be created.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<string> ImageToBase64StringForStorageAsync(Image objImageToSave, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return SavedImageQuality == int.MaxValue
                ? objImageToSave.ToBase64StringAsync(token: token)
                : objImageToSave.ToBase64StringAsJpegAsync(SavedImageQuality, token: token);
        }

        /// <summary>
        /// Last folder from which a mugshot was added
        /// </summary>
        public static string RecentImageFolder
        {
            get => _strImageFolder;
            set
            {
                if (Interlocked.Exchange(ref _strImageFolder, value) != value)
                {
                    using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
                        objRegistry?.SetValue("recentimagefolder", value);
                }
            }
        }

        #endregion Properties

        #region MRU Methods

        private static async Task LstFavoritedCharactersOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstFavoriteCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo),
                                    await s_LstFavoriteCharacters.GetValueAtAsync(i - 1, token).ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo),
                                    false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstFavoriteCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo),
                                    await s_LstFavoriteCharacters.GetValueAtAsync(i - 1, token).ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        string strNewFile = e.NewItems.Count > 0 ? e.NewItems[0] as string : string.Empty;
                        if (!string.IsNullOrEmpty(strNewFile))
                            s_ObjBaseChummerKey.SetValue(
                                "stickymru" + (e.OldStartingIndex + 1).ToString(InvariantCultureInfo), strNewFile);
                        else
                        {
                            for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if (i <= await s_LstFavoriteCharacters.GetCountAsync(token).ConfigureAwait(false))
                                    s_ObjBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo),
                                        await s_LstFavoriteCharacters.GetValueAtAsync(i - 1, token)
                                            .ConfigureAwait(false));
                                else
                                    s_ObjBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo),
                                        false);
                            }
                        }

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
                            s_ObjBaseChummerKey.SetValue("stickymru" + (i + 1).ToString(InvariantCultureInfo),
                                await s_LstFavoriteCharacters.GetValueAtAsync(i, token).ConfigureAwait(false));
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstFavoriteCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("stickymru" + i.ToString(InvariantCultureInfo),
                                    await s_LstFavoriteCharacters.GetValueAtAsync(i - 1, token).ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("stickymru" + i.ToString(InvariantCultureInfo), false);
                        }

                        break;
                    }
                }

                MruChanged?.Invoke(null, new TextEventArgs("stickymru"));
            }
            catch (System.Security.SecurityException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == nameof(Registry))
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        private static async Task LstMostRecentlyUsedCharactersOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = e.NewStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstMostRecentlyUsedCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo),
                                    await s_LstMostRecentlyUsedCharacters.GetValueAtAsync(i - 1, token)
                                        .ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstMostRecentlyUsedCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo),
                                    await s_LstMostRecentlyUsedCharacters.GetValueAtAsync(i - 1, token)
                                        .ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        string strNewFile = e.NewItems.Count > 0 ? e.NewItems[0] as string : string.Empty;
                        if (!string.IsNullOrEmpty(strNewFile))
                        {
                            s_ObjBaseChummerKey.SetValue(
                                "mru" + (e.OldStartingIndex + 1).ToString(InvariantCultureInfo), strNewFile);
                        }
                        else
                        {
                            for (int i = e.OldStartingIndex + 1; i <= MaxMruSize; ++i)
                            {
                                if (i <= await s_LstMostRecentlyUsedCharacters.GetCountAsync(token)
                                        .ConfigureAwait(false))
                                    s_ObjBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo),
                                        await s_LstMostRecentlyUsedCharacters.GetValueAtAsync(i - 1, token)
                                            .ConfigureAwait(false));
                                else
                                    s_ObjBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                            }
                        }

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
                            s_ObjBaseChummerKey.SetValue("mru" + (i + 1).ToString(InvariantCultureInfo),
                                await s_LstMostRecentlyUsedCharacters.GetValueAtAsync(i, token).ConfigureAwait(false));
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = 1; i <= MaxMruSize; ++i)
                        {
                            if (i <= await s_LstMostRecentlyUsedCharacters.GetCountAsync(token).ConfigureAwait(false))
                                s_ObjBaseChummerKey.SetValue("mru" + i.ToString(InvariantCultureInfo),
                                    await s_LstMostRecentlyUsedCharacters.GetValueAtAsync(i - 1, token)
                                        .ConfigureAwait(false));
                            else
                                s_ObjBaseChummerKey.DeleteValue("mru" + i.ToString(InvariantCultureInfo), false);
                        }

                        break;
                    }
                }

                MruChanged?.Invoke(null, new TextEventArgs("mru"));
            }
            catch (System.Security.SecurityException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == nameof(Registry))
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager
                        .GetStringAsync("Message_Insufficient_Permissions_Warning_Registry", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        public static MostRecentlyUsedCollection<string> FavoriteCharacters => s_LstFavoriteCharacters;

        public static MostRecentlyUsedCollection<string> MostRecentlyUsedCharacters => s_LstMostRecentlyUsedCharacters;

        public static LzmaHelper.ChummerCompressionPreset Chum5lzCompressionLevel
        {
            get => _eChum5lzCompressionLevel;
            set => _eChum5lzCompressionLevel = value;
        }

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

        #endregion MRU Methods
    }
}
