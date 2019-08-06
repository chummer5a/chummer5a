using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MersenneTwister;
using Microsoft.ApplicationInsights.Extensibility;
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
		public frmChummerMain MainForm { get; set; }

		private string _strLanguage = "en-us";

		/// <summary>
		/// Language.
		/// </summary>
		[SavePropertyAs("language")]
        [DropDown("Chummer.Classes.OptionsFileSystemHelper.GetListOfLanguages")]
        public string Language
		{
			get => _strLanguage;
			set
			{
				if (value != _strLanguage)
				{
					_strLanguage = value;
					try
					{
						CultureInfo = CultureInfo.GetCultureInfo(value);
					}
					catch (CultureNotFoundException)
					{
						CultureInfo = CultureInfo.CurrentCulture;
					}
				}
			}
		}

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
		/// Default character sheet to use when printing.
		/// </summary>
		[DropDown("Chummer.Classes.OptionsFileSystemHelper.GetListOfCharacterSheets")]
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
		[DropDown("Chummer.Classes.PDFParametersHelper.GetListOfPDFParameters")]
		public string PDFParameters { get; set; }

		/// <summary>
		/// List of SourcebookInfo.
		/// </summary>
        [DisplayIgnore] //TODO: No idea what I thought when this was added, but I suspect it isn't supposed to be shown directly and instead be a part of books?
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



        //TODO: All of this should be in one of the above regions, but I have not quite decided on which yet
        //Or I was just lazy and didn't sort them

        /// <summary>
        /// List of CustomDataDirectoryInfo.
        /// </summary>
        [DisplayIgnore]
        public List<CustomDataDirectoryInfo> CustomDataDirectoryInfo { get; } = new List<CustomDataDirectoryInfo>();

        /// <summary>
        /// Should Chummer present Easter Eggs to the user?
        /// </summary>
        public bool AllowEasterEggs { get; set; }

        [DisplayIgnore]
		public bool LiveCustomData { get; set; }

        [DisplayIgnore]
        public ObservableCollection<string> MostRecentlyUsedCharacters = new MostRecentlyUsedCollection<string>(GlobalOptions.MaxMruSize);

        [DisplayIgnore]
        public ObservableCollection<string> FavoritedCharacters = new MostRecentlyUsedCollection<string>(GlobalOptions.MaxMruSize);

        /// <summary>
        /// Whether or not the Character Roster should be shown. If true, prevents the roster from being removed or hidden. 
        /// </summary>
        public bool HideCharacterRoster { get; set; }

        /// <summary>
        /// Should the Plugins-Directory be loaded and the tabPlugins be shown?
        /// </summary>
        public bool PluginsEnabled { get; set; }
        [DisplayIgnore]
        public Dictionary<string, bool> PluginsEnabledDic { get; } = new Dictionary<string, bool>();



        private bool _blnUseLoggingApplicationInsights;
        /// <summary>
        /// Whether or not the app should use logging.
        /// </summary>
        public bool UseLoggingApplicationInsights
        {
            get => _blnUseLoggingApplicationInsights;
            set
            {
                _blnUseLoggingApplicationInsights = value;
                TelemetryConfiguration.Active.DisableTelemetry = !value;
            }
        }

        //TODO: Is this re-implementing exceptions?
        public string ErrorMessage { get; } = string.Empty;

        /// <summary>
        /// Which version of the Internet Explorer's rendering engine will be emulated for rendering the character view. Defaults to 8
        /// </summary>
        public int EmulatedBrowserVersion { get; set; } = 8;


        public bool LiveUpdateCleanCharacterFiles { get; set; }

        //is apparently stored here, for reasons

        [DisplayIgnore]
        public ThreadSafeRandom RandomGenerator { get; } = new ThreadSafeRandom(DsfmtRandom.Create(DsfmtEdition.OptGen_216091));

        [DisplayIgnore]
		public CultureInfo InvariantCultureInfo { get; } = System.Globalization.CultureInfo.InvariantCulture;

		/// <summary>
		/// CultureInfo for number localization.
		/// </summary>
		[DisplayIgnore]
		public CultureInfo CultureInfo { get; private set; } = CultureInfo.CurrentCulture;






    }

    public class CustomDataDirectoryInfo : IComparable
    {
        #region Properties

        public string Name { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is CustomDataDirectoryInfo objOtherDirectoryInfo)
            {
                int intReturn = string.Compare(Name, objOtherDirectoryInfo.Name, StringComparison.Ordinal);
                if (intReturn == 0)
                {
                    intReturn = string.Compare(Path, objOtherDirectoryInfo.Path, StringComparison.Ordinal);
                    if (intReturn == 0)
                    {
                        intReturn = Enabled == objOtherDirectoryInfo.Enabled ? 0 : (Enabled ? -1 : 1);
                    }
                }

                return intReturn;
            }

            return string.Compare(Name, obj.ToString(), StringComparison.Ordinal);
        }
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
