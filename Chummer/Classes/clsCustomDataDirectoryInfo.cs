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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// This class holds all information about an CustomDataDirectory
    /// </summary>
    public class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>, IComparable<CustomDataDirectoryInfo>
    {
        private readonly decimal _version;
        private readonly Guid _guid;
        private bool _isCreated;
        private readonly Exception _xmlException;

        private readonly List<DirectoryDependency> _lstDependencies = new List<DirectoryDependency>();
        private readonly List<DirectoryDependency> _lstExclusivities = new List<DirectoryDependency>();

        private readonly Dictionary<string, bool> _authorDictionary = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _descriptionDictionary = new Dictionary<string, string>();

        public CustomDataDirectoryInfo(string strName, string strPath)
        {
            Name = strName;
            Path = strPath;

            //Load all the needed data from xml and form the correct strings
            try
            {
                XmlDocument xmlObjManifest = new XmlDocument();
                string strFullDirectory = System.IO.Path.Combine(Path, "manifest.xml");

                if (File.Exists(strFullDirectory))
                {
                    xmlObjManifest.LoadStandard(strFullDirectory);

                    var xmlNode = xmlObjManifest.SelectSingleNode("manifest");

                    xmlNode.TryGetDecFieldQuickly("version", ref _version);
                    xmlNode.TryGetGuidFieldQuickly("guid", ref _guid);

                    GetManifestDescriptions(xmlNode);
                    GetManifestAuthors(xmlNode);
                    GetDependencies(xmlNode);
                    GetExclusivities(xmlNode);
                    DisplayName = GetDisplayName();
                }
            }
            catch (Exception ex)
            {
                //we save the exception to show it when opening CharacterOptions, see comment in LazyCreate() for the reasoning.
                _xmlException = ex;
            }

            #region Local Methods
            string GetDisplayName()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Name);
                sb.Append(" ");
                sb.Append("(");
                sb.Append(Version.ToString(GlobalOptions.CultureInfo));
                sb.Append(")");
                return sb.ToString();
            }
            void GetManifestDescriptions(XmlNode xmlDocument)
            {
                XmlNodeList xmlDescriptionNodes = xmlDocument.SelectNodes("descriptions/description");

                if (xmlDescriptionNodes == null)
                    return;

                foreach (XmlNode descriptionNode in xmlDescriptionNodes)
                {
                    string text = string.Empty;
                    string language = string.Empty;

                    descriptionNode.TryGetStringFieldQuickly("text", ref text);
                    descriptionNode.TryGetStringFieldQuickly("lang", ref language);

                    if (!string.IsNullOrEmpty(language))
                        _descriptionDictionary.Add(language, text);
                }
                //SetDisplayDescription();
            }
            //Must be called after DisplayDictionary was populated!

            void GetManifestAuthors(XmlNode xmlDocument)
            {
                XmlNodeList xmlAuthorNodes = xmlDocument.SelectNodes("authors/author");

                if (xmlAuthorNodes == null)
                    return;

                foreach (XmlNode objXmlNode in xmlAuthorNodes)
                {
                    string authorName = string.Empty;
                    bool isMain = false;

                    objXmlNode.TryGetStringFieldQuickly("name", ref authorName);
                    objXmlNode.TryGetBoolFieldQuickly("main", ref isMain);

                    if (!string.IsNullOrEmpty(authorName))
                        //Maybe a stupid idea? But who would add two authors with the same name anyway?
                        _authorDictionary.Add(authorName, isMain);
                }
                //After the list is fully formed, set the display author
                //SetDisplayAuthors();
            }
            //Must be called after AuthorDictionary was populated!

            void GetDependencies(XmlNode xmlDocument)
            {
                XmlNodeList xmlDependencies = xmlDocument.SelectNodes("dependencies/dependency");

                if (xmlDependencies == null)
                    return;

                foreach (XmlNode objXmlNode in xmlDependencies)
                {
                    Guid depGuid = Guid.Empty;
                    var newMaximumVersion = decimal.MaxValue; //If no upper limit is given, assume the maximum possible version
                    var newMinimumVersion = decimal.MinValue; //If no lower limit is given, assume the lowest possible version

                    string newName = string.Empty;

                    objXmlNode.TryGetStringFieldQuickly("name", ref newName);
                    objXmlNode.TryGetDecFieldQuickly("maxversion", ref newMaximumVersion);
                    objXmlNode.TryGetDecFieldQuickly("minversion", ref newMinimumVersion);
                    objXmlNode.TryGetGuidFieldQuickly("guid", ref depGuid);

                    //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                    if (string.IsNullOrEmpty(newName) || depGuid == Guid.Empty)
                        continue;

                    DirectoryDependency newDependency = new DirectoryDependency(newName, depGuid, newMinimumVersion, newMaximumVersion);

                    _lstDependencies.Add(newDependency);
                }
            }
            void GetExclusivities(XmlNode xmlDocument)
            {
                XmlNodeList xmlExclusivities = xmlDocument.SelectNodes("exclusivities/exclusivity");

                if (xmlExclusivities == null)
                    return;

                foreach (XmlNode objXmlNode in xmlExclusivities)
                {
                    Guid depGuid = Guid.Empty;
                    var newMaximumVersion = decimal.MaxValue; //If no upper limit is given, assume the maximum possible version
                    var newMinimumVersion = decimal.MinValue; //If no lower limit is given, assume the lowest possible version
                    string newName = string.Empty;

                    objXmlNode.TryGetStringFieldQuickly("name", ref newName);
                    objXmlNode.TryGetDecFieldQuickly("maxversion", ref newMaximumVersion);
                    objXmlNode.TryGetDecFieldQuickly("minversion", ref newMinimumVersion);
                    objXmlNode.TryGetGuidFieldQuickly("guid", ref depGuid);

                    //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                    if (string.IsNullOrEmpty(newName) || depGuid == Guid.Empty)
                        continue;

                    DirectoryDependency newExclusivity = new DirectoryDependency(newName, depGuid, newMinimumVersion, newMaximumVersion);
                    _lstExclusivities.Add(newExclusivity);
                }
            }
            #endregion
        }

        /// <summary>
        /// Contains all needed methods, that depend on LanguageManager.GetString and can't be called in the constructor or an exception will occur.
        /// </summary>
        public void LazyCreate()
        {
            //Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
            //and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after 
            //SetProcessDPI(GlobalOptions.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
            //the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterOptions are opened.

            //any message box creation in the constructor will violently kill chummer, because the CultureInfo was not set at that point in time. For that we show the massage, when opening CharacterOptions
            if (_xmlException != default)
            {
                Program.MainForm.ShowMessageBox(
                    string.Format(GlobalOptions.CultureInfo,
                        LanguageManager.GetString("Message_FailedLoad"), _xmlException.Message),
                    string.Format(GlobalOptions.CultureInfo,
                        LanguageManager.GetString("MessageTitle_FailedLoad")
                        + " " + Name),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_isCreated)
                return;

            var strFullDirectory = System.IO.Path.Combine(Path, "manifest.xml");
            if (!File.Exists(strFullDirectory))
            {
                _isCreated = true;
                DisplayDescription = LanguageManager.GetString("Tooltip_CharacterOptions_ManifestMissing");
                return;
            }
            
            try
            {
                DisplayAuthors = SetDisplayAuthors();
                DisplayDescription = SetDisplayDescription();
            }
            finally
            {
                _isCreated = true;
            }


            string SetDisplayDescription()
            {
                bool blnDefaultedToEng = false;

                DescriptionDictionary.TryGetValue(GlobalOptions.Language, out string description);

                if (string.IsNullOrEmpty(description))
                {
                    DescriptionDictionary.TryGetValue("en-us", out description);
                    blnDefaultedToEng = true;
                }

                if (string.IsNullOrEmpty(description))
                    return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestDescriptionMissing");

                if (blnDefaultedToEng)
                    description =
                        LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing") +
                        Environment.NewLine + Environment.NewLine + description;

                return description;
            }
            string SetDisplayAuthors()
            {
                List<string> authorsList = new List<string>();
                foreach (KeyValuePair<string, bool> kvp in AuthorDictionary)
                {
                    string formattedName = kvp.Key;

                    if (kvp.Value)
                        formattedName = kvp.Key + LanguageManager.GetString("IsMainAuthor");

                    authorsList.Add(formattedName);
                }
                return string.Join(", ", authorsList.ToArray());
            }
        }

        /* This is unused right now, but maybe we need it later for some reason.
        /// <summary>
        /// Calculates a combined SHA512 Hash from all files, that are not the manifest.xml and returns it as an int.
        /// </summary>
        /// <returns></returns>
        private int CalculateHash()
        {
            string[] strFiles = Directory.GetFiles(Path);
            byte[] allHashes = new byte[0];
            byte[] achrCombinedHashes;

            using (var sha512 = SHA512.Create())
            {
                foreach (var file in strFiles)
                {
                    if (file.Contains("manifest"))
                       continue;

                    using (var stream = File.OpenRead(file))
                    {
                        byte[] btyNewHash = sha512.ComputeHash(stream);
                        allHashes = allHashes.Concat(btyNewHash).ToArray();
                    }

                }
                achrCombinedHashes = sha512.ComputeHash(allHashes);
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(achrCombinedHashes);

            int intHash = BitConverter.ToInt32(achrCombinedHashes, 0);

            return intHash;
        }*/

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the dependent upon directory
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public string CheckDependency(CharacterOptions objCharacterOptions)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DirectoryDependency dependency in DependenciesList)
            {
                if (objCharacterOptions.EnabledCustomDataDirectoryInfos.All(enabledDirectory =>
                    enabledDirectory.Guid != dependency.UniqueIdentifier))
                {
                    sb.Append(dependency.DisplayName);
                    sb.Append(Environment.NewLine);

                    //We don't even need to attempt to check any versions if all guids are mismatched
                    continue;
                }

                //If not all GUIDs are unequal there has to be some version of an dependency active and we need to check it's version.
                foreach (var enabledCustomData in objCharacterOptions.EnabledCustomDataDirectoryInfos.Where(activeDirectory => activeDirectory.Guid == dependency.UniqueIdentifier))
                {
                    if (enabledCustomData.Version < dependency.MinimumVersion || enabledCustomData.Version > dependency.MaximumVersion)
                    {
                        sb.AppendFormat(LanguageManager.GetString("Tooltip_Dependency_VersionMismatch"), enabledCustomData.DisplayName, dependency.DisplayName);
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            return string.IsNullOrEmpty(sb.ToString()) ? string.Empty : sb.ToString();
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the prohibited directories
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public string CheckExclusivity(CharacterOptions objCharacterOptions)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var exclusivity in ExclusivitiesList)
            {
                //If all IDs are different we don't need to check any further
                if (objCharacterOptions.EnabledCustomDataDirectoryInfos.All(enabledDirectory => enabledDirectory.Guid != exclusivity.UniqueIdentifier)) continue;

                foreach (var enabledCustomData in objCharacterOptions.EnabledCustomDataDirectoryInfos.Where(activeDirectory => activeDirectory.Guid == exclusivity.UniqueIdentifier))
                {
                    //if the version is within the version range add it to the list.
                    if (enabledCustomData.Version > exclusivity.MinimumVersion && enabledCustomData.Version < exclusivity.MaximumVersion)
                    {
                        sb.Append(string.Format(LanguageManager.GetString("Tooltip_Exclusivity_VersionMismatch"), enabledCustomData.DisplayName, exclusivity.DisplayName));
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            return string.IsNullOrEmpty(sb.ToString()) ? string.Empty : sb.ToString();
        }

        /// <summary>
        /// Creates a string that displays which dependencies are missing or shouldn't be active to be displayed a tooltip.
        /// </summary>
        /// <param name="missingDependency">The string of all missing Dependencies</param>
        /// <param name="presentExclusivities">The string of all exclusivities that are active</param>
        /// <returns></returns>
        public static string BuildExclusivityDependencyString(string missingDependency = "", string presentExclusivities = "")
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(missingDependency))
            {
                sb.Append(LanguageManager.GetString("Tooltip_Dependency_Missing"));
                sb.Append(Environment.NewLine);
                sb.Append(missingDependency);
                sb.Append(Environment.NewLine);
            }
            if (!string.IsNullOrEmpty(presentExclusivities))
            {
                sb.Append(LanguageManager.GetString("Tooltip_Exclusivity_Present"));
                sb.Append(Environment.NewLine);
                sb.Append(presentExclusivities);
            }
            var formedString = sb.ToString();

            return string.IsNullOrEmpty(formedString) ? string.Empty : formedString;
        }



        #region Properties
        /// <summary>
        /// The name of the custom data directory
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The path to the Custom Data Directory
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The version of the custom data directory
        /// </summary>
        public decimal Version => _version;


        // /// <summary>
        // /// The Sha512 Hash of all non manifest.xml files in the directory
        // /// </summary>
        // public int Hash { get; private set; }

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(guid, str name, int version, bool isMinimumVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> DependenciesList => _lstDependencies;

        /// <summary>
        /// A list of all exclusivities each formatted as Tuple(int guid, str name, int version, bool ignoreVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> ExclusivitiesList => _lstExclusivities;

        /// <summary>
        /// A Dictionary containing all Descriptions, which uses the language code as key
        /// </summary>
        public IReadOnlyDictionary<string, string> DescriptionDictionary => _descriptionDictionary;

        /// <summary>
        /// The description to display
        /// </summary>
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// A Dictionary, that uses the author name as key and provides a bool if he is the main author
        /// </summary>
        public IReadOnlyDictionary<string, bool> AuthorDictionary => _authorDictionary;

        /// <summary>
        /// A string containing all Authors formatted as Author(main), Author2 
        /// </summary>
        public string DisplayAuthors { get; private set; }

        public Guid Guid => _guid;

        /// <summary>
        /// The name including the Version in this format "NAME (Version)"
        /// </summary>
        public string DisplayName { get; }
        #endregion

        #region Interface Implementations and Operators
        public int CompareTo(object obj)
        {
            if (obj is CustomDataDirectoryInfo objOtherDirectoryInfo)
                return CompareTo(objOtherDirectoryInfo);
            return 1;
        }

        public int CompareTo(CustomDataDirectoryInfo other)
        {
            int intReturn = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (intReturn == 0)
                intReturn = string.Compare(Path, other.Path, StringComparison.Ordinal);
            //Should basically never happen, because paths are supposed to be unique. But this "future proofs" it.
            if (intReturn == 0)
                intReturn = string.Compare(Version.ToString(GlobalOptions.CultureInfo), other.Version.ToString(GlobalOptions.CultureInfo), StringComparison.Ordinal);
            
            return intReturn;
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomDataDirectoryInfo objOther)
                return Equals(objOther);
            return false;
        }

        public override int GetHashCode()
        {
            //Path and Guid should already be enough because they are both unique, but just to be sure.
            return new { Name, Path, Guid, Version, DependenciesList, ExclusivitiesList }.GetHashCode();
        }

        public bool Equals(CustomDataDirectoryInfo other)
        {
            //This should be enough to uniquely identify an object.
            return other != null && Name == other.Name && Path == other.Path && other.Guid == Guid && other.Version == Version;
        }

        public static bool operator ==(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            return !(left == right);
        }

        public static bool operator <(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(CustomDataDirectoryInfo left, CustomDataDirectoryInfo right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        #endregion
    }

    /// <summary>
    /// Holds all information about an Dependency or Exclusivity
    /// </summary>
    public class DirectoryDependency
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <param name="minimumversion"></param>
        /// <param name="maximumversion"></param>
        public DirectoryDependency(string name, Guid guid, decimal minimumversion, decimal maximumversion)
        {
            Name = name;
            UniqueIdentifier = guid;
            MinimumVersion = minimumversion;
            MaximumVersion = maximumversion;
            DisplayName = GetDisplayName();

            string GetDisplayName()
            {
                StringBuilder sb = new StringBuilder();

                //If neither min and max version are given, just display the Name instead of the decimal.min and decimal.max
                if (MinimumVersion == decimal.MinValue && MaximumVersion == decimal.MaxValue)
                {
                    return Name;
                }

                //If no maxversion is not given, don't display decimal.max display > instead
                if (MinimumVersion != decimal.MinValue && MaximumVersion == decimal.MaxValue)
                {
                    sb.Append(Name);
                    sb.Append(" ");
                    sb.Append("(");
                    sb.Append(" > ");
                    sb.Append(MinimumVersion.ToString(GlobalOptions.CultureInfo));
                    sb.Append(")");
                    return sb.ToString();
                }

                //If no minversion is not given, don't display decimal.min display < instead
                if (MinimumVersion == decimal.MinValue && MaximumVersion != decimal.MaxValue)
                {
                    sb.Append(Name);
                    sb.Append(" ");
                    sb.Append("(");
                    sb.Append(" < ");
                    sb.Append(MaximumVersion.ToString(GlobalOptions.CultureInfo));
                    sb.Append(")");
                    return sb.ToString();
                }

                sb.Append(Name);
                sb.Append(" ");
                sb.Append("(");
                sb.Append(MinimumVersion.ToString(GlobalOptions.CultureInfo));
                sb.Append(" - ");
                sb.Append(MaximumVersion.ToString(GlobalOptions.CultureInfo));
                sb.Append(")");
                return sb.ToString();
            }
        }

        public string Name { get; }
        public Guid UniqueIdentifier { get; }
        public decimal MinimumVersion { get; }
        public decimal MaximumVersion { get; }
        public string DisplayName { get; }
    }
    
}
