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
    public sealed class ProgramOptions
    {
        
        // Omae Information.

        
        
        // PDF information.

        #region Constructor and Instance

        static ProgramOptions()
        {
            
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
        [DropDown(" Chummer.Classes.PDFParametersHelper.GetListOfPDFParameters")]
        public string PDFParameters { get; set; }

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

        //TODO: Make sure it gets saved
        [DisplayIgnore]
        public Version SavedByVersion { get; set; }

        #endregion

        #region Imported Character

        /// <summary>
		/// Whether or not confirmation messages are shown when deleting an object.
		/// </summary>
		//[OptionAttributes("OptionHeader_ProgramOptions")]
        [SavePropertyAs("confirmdelete")]
        public bool ConfirmDelete { get; set; } = true;

        /// <summary>
        /// Wehther or not confirmation messages are shown for Karma Expenses.
        /// </summary>
        [SavePropertyAs("confirmkarmaexpense")]
        public bool ConfirmKarmaExpense { get; set; } = true;

        /// <summary>
        /// Whether or not a backup copy of the character should be created before they are placed into Career Mode.
        /// </summary>
        [SavePropertyAs("createbackuponcareer")]
        public bool CreateBackupOnCareer { get; set; }

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        [SavePropertyAs("essencedecimals")]
        public int EssenceDecimals { get; set; } = 2;

        /// <summary>
        /// Default build method.
        /// </summary>
        [SavePropertyAs("buildmethod")]
        public CharacterBuildMethod BuildMethod { get; set; } = CharacterBuildMethod.Priority;

        /// <summary>
        /// Default number of build points.
        /// </summary>
        [SavePropertyAs("buildpoints")]
        public int BuildPoints { get; set; } = 800;

        /// <summary>
        /// Default Availability.
        /// </summary>
        [SavePropertyAs("availability")]
        public int Availability { get; set; } = 12;

        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        [SavePropertyAs("autobackstory")]
        public bool AutomaticBackstory { get; internal set; } = true;

        #endregion

        #region Printing
        /// <summary>
        /// Whether or not all Active Skills with a total score higher than 0 should be printed.
        /// </summary>
        [OptionAttributes("OptionHeader_GlobalOptions/Display_PrintingOptions")]
        [SavePropertyAs("printzeroratingskills")]
        public bool PrintSkillsWithZeroRating { get; set; } = true;

        /// <summary>
        /// Whether or not the Karma and Nueyn Expenses should be printed on the character sheet.
        /// </summary>
        [SavePropertyAs("printexpenses")]
        public bool PrintExpenses { get; set; }

        /// <summary>
        /// Whether or not Notes should be printed.
        /// </summary>
        [SavePropertyAs("printnotes")]
        public bool PrintNotes { get; set; }
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