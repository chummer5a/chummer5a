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
﻿using System.Xml;
using System.Windows.Forms;
 using Chummer.Backend.Equipment;
 using Microsoft.Win32;

// MRUChanged Event Handler.
public delegate void MRUChangedHandler();

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
		string _strCode = "";
		string _strPath = "";
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

	/// <summary>
	/// Global Options. A single instance class since Options are common for all characters, reduces execution time and memory usage.
	/// </summary>
	public sealed class GlobalOptions
	{
		static readonly GlobalOptions _objInstance = new GlobalOptions();
		static readonly CultureInfo _objCultureInfo = CultureInfo.InvariantCulture;

		public event MRUChangedHandler MRUChanged;

		private frmMain _frmMainForm;

		private static bool _blnAutomaticUpdate = false;
		private static bool _blnLocalisedUpdatesOnly = false;
		private static bool _blnStartupFullscreen = false;
		private static bool _blnSingleDiceRoller = true;
		private static string _strLanguage = "en-us";
		private static string _strDefaultCharacterSheet = "Shadowrun 5";
		private static bool _blnDatesIncludeTime = true;
		private static bool _blnPrintToFileFirst = false;
		private static bool _lifeModuleEnabled;
		private static bool _blnMissionsOnly = false;
		private static bool _blnDronemods = false;
		private static bool _blnPreferNightlyUpdates = false;

		// Omae Information.
		private static bool _omaeEnabled = false;
		private static string _strOmaeUserName = "";
		private static string _strOmaePassword = "";
		private static bool _blnOmaeAutoLogin = false;

		private XmlDocument _objXmlClipboard = new XmlDocument();
		private ClipboardContentType _objClipboardContentType = new ClipboardContentType();

		public static GradeList CyberwareGrades = new GradeList();
		public static GradeList BiowareGrades = new GradeList();

		// PDF information.
		public static string _strPDFAppPath = "";
        public static string _strPDFParameters = "";
		public static List<SourcebookInfo> _lstSourcebookInfo = new List<SourcebookInfo>();
        public static bool _blnUseLogging = false;
		private static string _strCharacterRosterPath;

		#region Constructor and Instance
		static GlobalOptions()
		{
			if (Utils.IsRunningInVisualStudio()) return;

			string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
			if (!Directory.Exists(settingsDirectoryPath))
				Directory.CreateDirectory(settingsDirectoryPath);

			// Automatic Update.
			try
			{
				_blnAutomaticUpdate = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("autoupdate").ToString());
			}
			catch
			{
			}

			try
			{
				_lifeModuleEnabled =
					Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("lifemodule").ToString());
			}
			catch 
			{
			}

			try
			{
				_omaeEnabled =
					Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaeenabled").ToString());
			}
			catch 
			{
			}

            // Whether or not the app should only download localised files in the user's selected language.
			try
			{
				_blnLocalisedUpdatesOnly = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("localisedupdatesonly").ToString());
			}
			catch
			{
			}

            // Whether or not the app should use logging.
            try
            {
                _blnUseLogging = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("uselogging").ToString());
            }
            catch
            {
            }

            // Whether or not dates should include the time.
			try
			{
				_blnDatesIncludeTime = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("datesincludetime").ToString());
			}
			catch
			{
			}

			try
			{
				_blnMissionsOnly = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("missionsonly").ToString());
			}
			catch { }

			try
			{
				_blnDronemods = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("dronemods").ToString());
			}
			catch { }


			// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
			try
			{
				_blnPrintToFileFirst = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("printtofilefirst").ToString());
			}
			catch
			{
			}

			// Default character sheet.
			try
			{
				_strDefaultCharacterSheet = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("defaultsheet").ToString();
			}
			catch
			{
			}

			// Omae Settings.
			// Username.
			try
			{
				_strOmaeUserName = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaeusername").ToString();
			}
			catch
			{
			}
			// Password.
			try
			{
				_strOmaePassword = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaepassword").ToString();
			}
			catch
			{
			}
			// AutoLogin.
			try
			{
				_blnOmaeAutoLogin = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("omaeautologin").ToString());
			}
			catch
			{
			}
			// Language.
			try
			{
				_strLanguage = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("language").ToString();
				if (_strLanguage == "en-us2")
				{
					_strLanguage = "en-us";
				}
			}
			catch
			{
			}
			// Startup in Fullscreen mode.
			try
			{
				_blnStartupFullscreen = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("startupfullscreen").ToString());
			}
			catch
			{
			}
			// Single instace of the Dice Roller window.
			try
			{
				_blnSingleDiceRoller = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("singlediceroller").ToString());
			}
			catch
			{
			}

            // Open PDFs as URLs. For use with Chrome, Firefox, etc.
            try
            {
                _strPDFParameters = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("pdfparameters").ToString();
            }
            catch
            {
            }

            // PDF application path.
            try
			{
				_strPDFAppPath = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("pdfapppath").ToString();
			}
			catch
			{
			}

			// Folder path to check for characters.
			try
			{
				_strCharacterRosterPath = Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("characterrosterpath").ToString();
			}
			catch
			{
			}

			// Prefer Nightly Updates.
			try
			{
				_blnPreferNightlyUpdates = Convert.ToBoolean(Registry.CurrentUser.CreateSubKey("Software\\Chummer5").GetValue("prefernightlybuilds").ToString());
			}
			catch
			{
			}

			// Retrieve the SourcebookInfo objects.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("books.xml");
			XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
			foreach (XmlNode objXmlBook in objXmlBookList)
			{
				try
				{
					SourcebookInfo objSource = new SourcebookInfo();
					string strTemp = Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Sourcebook").GetValue(objXmlBook["code"].InnerText).ToString();
					string[] strParts = strTemp.Split('|');
					objSource.Code = objXmlBook["code"].InnerText;
					objSource.Path = strParts[0];
					objSource.Offset = Convert.ToInt32(strParts[1]);

					_lstSourcebookInfo.Add(objSource);
				}
				catch
				{
				}
			}

			CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware);
			BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware);
		}

		

		/// <summary>
		/// Global instance of the GlobalOptions.
		/// </summary>
		public static GlobalOptions Instance
		{
			get
			{
				return _objInstance;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not Automatic Updates are enabled.
		/// </summary>
		public bool AutomaticUpdate
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

		public bool LifeModuleEnabled
		{
			get { return _lifeModuleEnabled; }
			set { _lifeModuleEnabled = value; }
		}

        /// <summary>
		/// Whether or not the app should only download localised files in the user's selected language.
		/// </summary>
		public bool LocalisedUpdatesOnly
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
        public bool UseLogging
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
		public bool DatesIncludeTime
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

		public bool MissionsOnly
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

		public bool Dronemods
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


		/// <summary>
		/// Whether or not printouts should be sent to a file before loading them in the browser. This is a fix for getting printing to work properly on Linux using Wine.
		/// </summary>
		public bool PrintToFileFirst
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
		public string OmaeUserName
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
		public string OmaePassword
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
		public bool OmaeAutoLogin
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
		public frmMain MainForm
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
		public string Language
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
		public bool StartupFullscreen
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
		public bool SingleDiceRoller
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
		/// CultureInfor for number localization.
		/// </summary>
		public CultureInfo CultureInfo
		{
			get
			{
				return _objCultureInfo;
			}
		}

		/// <summary>
		/// Clipboard.
		/// </summary>
		public XmlDocument Clipboard
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
		public ClipboardContentType ClipboardContentType
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
		public string DefaultCharacterSheet
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
		public string PDFAppPath
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

		public string PDFParameters
		{
			get { return _strPDFParameters;}
			set { _strPDFParameters = value; }
		}
		/// <summary>
		/// List of SourcebookInfo.
		/// </summary>
		public List<SourcebookInfo> SourcebookInfo
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

		public bool OmaeEnabled
		{
			get { return _omaeEnabled; }
			set { _omaeEnabled = value; }
		}

		public bool PreferNightlyBuilds
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

		public string CharacterRosterPath
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

		public string PDFArguments { get; internal set; }
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