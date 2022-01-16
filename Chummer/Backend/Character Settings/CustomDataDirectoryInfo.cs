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
using System.Text;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// This class holds all information about an CustomDataDirectory
    /// </summary>
    public sealed class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>, IComparable<CustomDataDirectoryInfo>, IHasInternalId
    {
        private readonly Version _objMyVersion = new Version(1, 0);
        private Guid _guid = Guid.NewGuid();

        public Exception XmlException { get; }

        public bool HasManifest { get; }

        private readonly List<DirectoryDependency> _lstDependencies = new List<DirectoryDependency>();
        private readonly List<DirectoryDependency> _lstIncompatibilities = new List<DirectoryDependency>();

        private readonly Dictionary<string, bool> _authorDictionary = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _descriptionDictionary = new Dictionary<string, string>();

        public CustomDataDirectoryInfo(string strName, string strDirectoryPath)
        {
            Name = strName;
            DirectoryPath = strDirectoryPath;

            //Load all the needed data from xml and form the correct strings
            try
            {
                string strFullDirectory = Path.Combine(DirectoryPath, "manifest.xml");

                if (File.Exists(strFullDirectory))
                {
                    HasManifest = true;
                    XmlDocument xmlObjManifest = new XmlDocument();
                    xmlObjManifest.LoadStandard(strFullDirectory);
                    var xmlNode = xmlObjManifest.SelectSingleNode("manifest");

                    if (!xmlNode.TryGetField("version", VersionExtensions.TryParse, out _objMyVersion))
                        _objMyVersion = new Version(1, 0);
                    xmlNode.TryGetGuidFieldQuickly("guid", ref _guid);

                    GetManifestDescriptions(xmlNode);
                    GetManifestAuthors(xmlNode);
                    GetDependencies(xmlNode);
                    GetIncompatibilities(xmlNode);
                }
            }
            catch (Exception ex)
            {
                // Save the exception to show it later
                XmlException = ex;
                HasManifest = false;
            }

            #region Local Methods

            void GetManifestDescriptions(XmlNode xmlDocument)
            {
                XmlNodeList xmlDescriptionNodes = xmlDocument.SelectNodes("descriptions/description");

                if (xmlDescriptionNodes == null)
                    return;

                foreach (XmlNode descriptionNode in xmlDescriptionNodes)
                {
                    string language = string.Empty;
                    descriptionNode.TryGetStringFieldQuickly("lang", ref language);

                    if (!string.IsNullOrEmpty(language))
                    {
                        string text = string.Empty;
                        if (descriptionNode.TryGetStringFieldQuickly("text", ref text))
                            _descriptionDictionary.Add(language, text);
                    }
                }
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

                    if (!string.IsNullOrEmpty(authorName) && !_authorDictionary.ContainsKey(authorName))
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
                    Guid guidId = Guid.Empty;
                    string strDependencyName = string.Empty;

                    objXmlNode.TryGetStringFieldQuickly("name", ref strDependencyName);
                    objXmlNode.TryGetGuidFieldQuickly("guid", ref guidId);

                    //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                    if (string.IsNullOrEmpty(strDependencyName) || guidId == Guid.Empty)
                        continue;

                    objXmlNode.TryGetField("maxversion", VersionExtensions.TryParse, out Version objNewMaximumVersion);
                    objXmlNode.TryGetField("minversion", VersionExtensions.TryParse, out Version objNewMinimumVersion);

                    DirectoryDependency objDependency = new DirectoryDependency(strDependencyName, guidId, objNewMinimumVersion, objNewMaximumVersion);
                    _lstDependencies.Add(objDependency);
                }
            }

            void GetIncompatibilities(XmlNode xmlDocument)
            {
                XmlNodeList xmlIncompatibilities = xmlDocument.SelectNodes("incompatibilities/incompatibility");

                if (xmlIncompatibilities == null)
                    return;

                foreach (XmlNode objXmlNode in xmlIncompatibilities)
                {
                    Guid guidId = Guid.Empty;
                    string strDependencyName = string.Empty;

                    objXmlNode.TryGetStringFieldQuickly("name", ref strDependencyName);
                    objXmlNode.TryGetGuidFieldQuickly("guid", ref guidId);

                    //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                    if (string.IsNullOrEmpty(strDependencyName) || guidId == Guid.Empty)
                        continue;

                    objXmlNode.TryGetField("maxversion", VersionExtensions.TryParse, out Version objNewMaximumVersion);
                    objXmlNode.TryGetField("minversion", VersionExtensions.TryParse, out Version objNewMinimumVersion);

                    DirectoryDependency objIncompatibility = new DirectoryDependency(strDependencyName, guidId, objNewMinimumVersion, objNewMaximumVersion);
                    _lstIncompatibilities.Add(objIncompatibility);
                }
            }

            #endregion Local Methods
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
        /// <param name="objCharacterSettings"></param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public string CheckDependency(CharacterSettings objCharacterSettings)
        {
            int intMyLoadOrderPosition = objCharacterSettings.EnabledCustomDataDirectoryInfos.FindIndex(x => x.Equals(this));
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                List<CustomDataDirectoryInfo> lstEnabledCustomData = new List<CustomDataDirectoryInfo>(DependenciesList.Count);
                foreach (DirectoryDependency dependency in DependenciesList)
                {
                    lstEnabledCustomData.Clear();
                    Guid objDependencyGuid = dependency.UniqueIdentifier;
                    if (objCharacterSettings.EnabledCustomDataDirectoryInfoGuids.Contains(objDependencyGuid))
                        lstEnabledCustomData.AddRange(
                            objCharacterSettings.EnabledCustomDataDirectoryInfos.Where(
                                x => x.Guid.Equals(objDependencyGuid)));
                    if (lstEnabledCustomData.Count > 0)
                    {
                        // First check if we have any data whose version matches
                        Version objMinVersion = dependency.MinimumVersion;
                        Version objMaxVersion = dependency.MaximumVersion;
                        if (objMinVersion != default || objMaxVersion != default)
                        {
                            bool blnMismatch;
                            if (objMinVersion != default && objMaxVersion != default)
                            {
                                blnMismatch = lstEnabledCustomData.All(
                                    x => x.MyVersion < objMinVersion
                                         || x.MyVersion > objMaxVersion);
                            }
                            else if (objMinVersion != default)
                            {
                                blnMismatch = lstEnabledCustomData.All(x => x.MyVersion < objMinVersion);
                            }
                            else
                            {
                                blnMismatch = lstEnabledCustomData.All(x => x.MyVersion > objMaxVersion);
                            }
                            if (blnMismatch)
                            {
                                sbdReturn.AppendFormat(LanguageManager.GetString("Tooltip_Dependency_VersionMismatch"),
                                                       lstEnabledCustomData[0].DisplayName, dependency.DisplayName)
                                         .AppendLine();
                                continue;
                            }
                            // Remove all from the list where version does not match before moving on to load orders
                            if (objMinVersion != default && objMaxVersion != default)
                            {
                                lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion
                                                                    || x.MyVersion > objMaxVersion);
                            }
                            else if (objMinVersion != default)
                            {
                                lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion);
                            }
                            else
                            {
                                lstEnabledCustomData.RemoveAll(x => x.MyVersion > objMaxVersion);
                            }
                        }

                        if (intMyLoadOrderPosition >= 0 && intMyLoadOrderPosition
                            < objCharacterSettings.EnabledCustomDataDirectoryInfos.FindLastIndex(
                                x => lstEnabledCustomData.Contains(x)))
                        {
                            sbdReturn.AppendFormat(LanguageManager.GetString("Tooltip_Dependency_BadLoadOrder"),
                                                   lstEnabledCustomData[0].Name, Name).AppendLine();
                        }
                    }
                    else
                    {
                        //We don't even need to attempt to check any versions if all guids are mismatched
                        sbdReturn.AppendLine(dependency.DisplayName);
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the prohibited directories
        /// </summary>
        /// <param name="objCharacterSettings"></param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public string CheckIncompatibility(CharacterSettings objCharacterSettings)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                List<CustomDataDirectoryInfo> lstEnabledCustomData = new List<CustomDataDirectoryInfo>(IncompatibilitiesList.Count);
                foreach (DirectoryDependency incompatibility in IncompatibilitiesList)
                {
                    Guid objIncompatibilityGuid = incompatibility.UniqueIdentifier;
                    //Use the fast HasSet.Contains to determine if any dependency is present
                    if (!objCharacterSettings.EnabledCustomDataDirectoryInfoGuids.Contains(
                            objIncompatibilityGuid))
                        continue;
                    //We still need to filter out all the matching incompatibilities from objCharacterSettings.EnabledCustomDataDirectoryInfos to check their versions
                    lstEnabledCustomData.Clear();
                    if (objCharacterSettings.EnabledCustomDataDirectoryInfoGuids.Contains(
                            objIncompatibilityGuid))
                        lstEnabledCustomData.AddRange(
                            objCharacterSettings.EnabledCustomDataDirectoryInfos.Where(
                                x => x.Guid.Equals(objIncompatibilityGuid)));
                    if (lstEnabledCustomData.Count == 0)
                        continue;
                    CustomDataDirectoryInfo objInfoToDisplay;
                    Version objMinVersion = incompatibility.MinimumVersion;
                    Version objMaxVersion = incompatibility.MaximumVersion;
                    if (objMinVersion != default)
                    {
                        if (incompatibility.MaximumVersion != default)
                            objInfoToDisplay = lstEnabledCustomData.Find(
                                x => x.MyVersion >= objMinVersion || x.MyVersion <= objMaxVersion);
                        else
                            objInfoToDisplay
                                = lstEnabledCustomData.Find(x => x.MyVersion >= objMinVersion);
                    }
                    else if (incompatibility.MaximumVersion != default)
                    {
                        objInfoToDisplay
                            = lstEnabledCustomData.Find(x => x.MyVersion <= objMaxVersion);
                    }
                    else
                        objInfoToDisplay = lstEnabledCustomData[0];

                    //if the version is within the version range add it to the list.
                    if (objInfoToDisplay != default)
                    {
                        sbdReturn.AppendFormat(LanguageManager.GetString("Tooltip_Incompatibility_VersionMismatch"),
                                               objInfoToDisplay.DisplayName, incompatibility.DisplayName).AppendLine();
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Creates a string that displays which dependencies are missing or shouldn't be active to be displayed a tooltip.
        /// </summary>
        /// <param name="missingDependency">The string of all missing Dependencies</param>
        /// <param name="presentIncompatibilities">The string of all incompatibilities that are active</param>
        /// <returns></returns>
        public static string BuildIncompatibilityDependencyString(string missingDependency = "", string presentIncompatibilities = "")
        {
            string strReturn = string.Empty;

            if (!string.IsNullOrEmpty(missingDependency))
            {
                strReturn = LanguageManager.GetString("Tooltip_Dependency_Missing") + Environment.NewLine + missingDependency;
            }

            if (!string.IsNullOrEmpty(presentIncompatibilities))
            {
                if (!string.IsNullOrEmpty(strReturn))
                    strReturn += Environment.NewLine;
                strReturn += LanguageManager.GetString("Tooltip_Incompatibility_Present") + Environment.NewLine + presentIncompatibilities;
            }

            return strReturn;
        }

        #region Properties

        /// <summary>
        /// The name of the custom data directory
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The path to the Custom Data Directory
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// The version of the custom data directory
        /// </summary>
        public Version MyVersion => _objMyVersion;

        // /// <summary>
        // /// The Sha512 Hash of all non manifest.xml files in the directory
        // /// </summary>
        // public int Hash { get; private set; }

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(guid, str name, int version, bool isMinimumVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> DependenciesList => _lstDependencies;

        /// <summary>
        /// A list of all incompatibilities each formatted as Tuple(int guid, str name, int version, bool ignoreVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> IncompatibilitiesList => _lstIncompatibilities;

        /// <summary>
        /// A Dictionary containing all Descriptions, which uses the language code as key
        /// </summary>
        public IReadOnlyDictionary<string, string> DescriptionDictionary => _descriptionDictionary;

        private string _strDisplayDescriptionLanguage = GlobalSettings.Language;

        private string _strDisplayDescription;

        /// <summary>
        /// The description to display
        /// </summary>
        public string DisplayDescription
        {
            get
            {
                // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
                // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
                // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
                // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
                // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
                if (_strDisplayDescription == null || _strDisplayDescriptionLanguage != GlobalSettings.Language)
                {
                    _strDisplayDescriptionLanguage = GlobalSettings.Language;
                    _strDisplayDescription = GetDisplayDescription(GlobalSettings.Language);
                }

                return _strDisplayDescription;
            }
        }

        public string GetDisplayDescription(string strLanguage)
        {
            if (!File.Exists(Path.Combine(DirectoryPath, "manifest.xml")))
                return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestMissing", strLanguage);

            if (DescriptionDictionary.TryGetValue(strLanguage, out string description))
                return description.NormalizeLineEndings(true);

            if (!DescriptionDictionary.TryGetValue(GlobalSettings.DefaultLanguage, out description))
                return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestDescriptionMissing", strLanguage);

            return LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing",
                                             strLanguage) +
                   Environment.NewLine + Environment.NewLine + description.NormalizeLineEndings(true);
        }

        /// <summary>
        /// A Dictionary, that uses the author name as key and provides a bool if he is the main author
        /// </summary>
        public IReadOnlyDictionary<string, bool> AuthorDictionary => _authorDictionary;

        private string _strDisplayAuthorsLanguage = GlobalSettings.Language;

        private string _strDisplayAuthors;

        /// <summary>
        /// A string containing all Authors formatted as Author(main), Author2
        /// </summary>
        public string DisplayAuthors
        {
            get
            {
                // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
                // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
                // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
                // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
                // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
                if (_strDisplayAuthors == null || _strDisplayAuthorsLanguage != GlobalSettings.Language)
                {
                    _strDisplayAuthorsLanguage = GlobalSettings.Language;
                    _strDisplayAuthors = GetDisplayAuthors(GlobalSettings.Language, GlobalSettings.CultureInfo);
                }

                return _strDisplayAuthors;
            }
        }

        public string GetDisplayAuthors(string strLanguage, CultureInfo objCultureInfo)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDisplayAuthors))
            {
                foreach (KeyValuePair<string, bool> kvp in AuthorDictionary)
                {
                    sbdDisplayAuthors.AppendLine(kvp.Value
                                                     ? string.Format(objCultureInfo, kvp.Key,
                                                                     LanguageManager.GetString(
                                                                         "String_IsMainAuthor", strLanguage))
                                                     : kvp.Key);
                }

                return sbdDisplayAuthors.ToString().Trim();
            }
        }

        public Guid Guid => _guid;

        public void RandomizeGuid()
        {
            // We should not be randomizing Guids for infos that have a manifest with a set Guid
            if (HasManifest)
                throw new NotSupportedException();
            _guid = Guid.NewGuid();
        }

        public void CopyGuid(CustomDataDirectoryInfo other)
        {
            // We should not be copying Guids for infos that have a manifest with a set Guid
            if (HasManifest)
                throw new NotSupportedException();
            _guid = other._guid;
        }

        /// <summary>
        /// The name including the Version in this format "NAME (Version)"
        /// </summary>
        public string DisplayName => MyVersion == default
            ? Name
            : string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", Name,
                            LanguageManager.GetString("String_Space"), MyVersion);

        public string InternalId => Guid.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Key to use in character options files
        /// </summary>
        public string CharacterSettingsSaveKey => HasManifest ? InternalId + '>' + MyVersion : Name;

        public static string GetIdFromCharacterSettingsSaveKey(string strKey)
        {
            int intSeparatorIndex = strKey.IndexOf('>');
            if (intSeparatorIndex >= 0 && intSeparatorIndex + 1 < strKey.Length)
            {
                string strReturn = strKey.Substring(0, intSeparatorIndex);
                if (strReturn.IsGuid())
                    return strReturn;
            }

            return string.Empty;
        }

        public static string GetIdFromCharacterSettingsSaveKey(string strKey, out Version objPreferredVersion)
        {
            int intSeparatorIndex = strKey.IndexOf('>');
            if (intSeparatorIndex >= 0 && intSeparatorIndex + 1 < strKey.Length)
            {
                string strReturn = strKey.Substring(0, intSeparatorIndex);
                if (strReturn.IsGuid())
                {
                    objPreferredVersion = new Version(strKey.Substring(intSeparatorIndex + 1));
                    return strReturn;
                }
            }

            objPreferredVersion = default;
            return string.Empty;
        }

        #endregion Properties

        #region Interface Implementations and Operators

        public int CompareTo(object obj)
        {
            if (obj is CustomDataDirectoryInfo objOtherDirectoryInfo)
                return CompareTo(objOtherDirectoryInfo);
            return 1;
        }

        public int CompareTo(CustomDataDirectoryInfo other)
        {
            int intReturn = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            if (intReturn == 0)
            {
                intReturn = string.Compare(DirectoryPath, other.DirectoryPath, StringComparison.OrdinalIgnoreCase);
                //Should basically never happen, because paths are supposed to be unique. But this "future proofs" it.
                if (intReturn == 0)
                    intReturn = MyVersion.CompareTo(other.MyVersion);
            }

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
            var dependencyHash = DependenciesList.GetEnsembleHashCode();
            var incompatibilityHash = IncompatibilitiesList.GetEnsembleHashCode();
            var guid = HasManifest ? Guid : Guid.Empty;
            //Path and Guid should already be enough because they are both unique, but just to be sure.
            return (Name, guid, DirectoryPath, MyVersion, incompatibilityHash, dependencyHash).GetHashCode();
        }

        public bool Equals(CustomDataDirectoryInfo other)
        {
            //This should be enough to uniquely identify an object.
            return other != null && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                   DirectoryPath.Equals(other.DirectoryPath, StringComparison.OrdinalIgnoreCase) &&
                   (other.Guid == Guid || (!other.HasManifest && !HasManifest)) && other.MyVersion.Equals(MyVersion) &&
                   other.DependenciesList.CollectionEqual(DependenciesList) &&
                   other.IncompatibilitiesList.CollectionEqual(IncompatibilitiesList);
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

        #endregion Interface Implementations and Operators
    }

    /// <summary>
    /// Holds all information about an Dependency or Incompatibility
    /// </summary>
    public class DirectoryDependency
    {
        public DirectoryDependency(string name, Guid guid, Version minVersion, Version maxVersion)
        {
            Name = name;
            UniqueIdentifier = guid;
            MinimumVersion = minVersion;
            MaximumVersion = maxVersion;
        }

        public string Name { get; }

        public Guid UniqueIdentifier { get; }

        public Version MinimumVersion { get; }

        public Version MaximumVersion { get; }

        public string DisplayName
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");

                if (MinimumVersion != default)
                {
                    return MaximumVersion != default
                        ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({2}{1}-{1}{3})", Name, strSpace, MinimumVersion, MaximumVersion)
                        // If maxversion is not given, don't display decimal.max display > instead
                        : string.Format(GlobalSettings.CultureInfo, "{0}{1}({1}>{1}{2})", Name, strSpace, MinimumVersion);
                }

                return MaximumVersion != default
                    // If minversion is not given, don't display decimal.min display < instead
                    ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({1}<{1}{2})", Name, strSpace, MaximumVersion)
                    // If neither min and max version are given, just display the Name instead of the decimal.min and decimal.max
                    : Name;
            }
        }
    }
}
