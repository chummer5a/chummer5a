using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>, IComparable<CustomDataDirectoryInfo>
    {
        private bool _blnCreated;
        private int _version;

        //Tuple<int Hash, string Name, int Version, bool isMinimumVersion>
        private readonly List<Tuple<Guid, string, int, bool>> _lstDependencies= new List<Tuple<Guid, string, int, bool>>();
        //Tuple<int Hash, string Name, int Version, bool ignoreVersion>
        private readonly List<Tuple<Guid, string, int, bool>> _lstExclusivities = new List<Tuple<Guid, string, int, bool>>();
        private Guid _guid;

        public CustomDataDirectoryInfo(string strName, string strPath)
        {
            Name = strName;
            Path = strPath;
            Create();
        }


        #region Create, Load and Save Methods

        #region Create() Helper Methods
        /// <summary>
        /// Reads in all descriptions from an given loaded manifest.xml and adds them to _lstDescriptions
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void GetManifestDescriptions(XmlNode xmlDocument)
        {
            XmlNodeList xmlDescriptionNodes = xmlDocument.SelectNodes("descriptions/description");

            if (xmlDescriptionNodes == null)
                return;

            foreach (XmlNode descriptionNode in xmlDescriptionNodes)
            {
                descriptionNode.TryGetField("text", out string text);
                descriptionNode.TryGetField("lang", out string language);

                if(!string.IsNullOrEmpty(language))
                    DescriptionDictionary.Add(language, text);
            }
            SetDisplayDescription();
        }

        /// <summary>
        /// Selects the description in the correct language and saves it to _strDisplayDescription
        /// </summary>
        private void SetDisplayDescription()
        {
            bool blnDefaultedToEng = false;

            DescriptionDictionary.TryGetValue(GlobalOptions.Language, out string description);

            if (string.IsNullOrEmpty(description))
            {
                DescriptionDictionary.TryGetValue("en-us", out description);
                blnDefaultedToEng = true;
            }

            if (string.IsNullOrEmpty(description))
            {
                DisplayDescription = LanguageManager.GetString("Tooltip_CharacterOptions_ManifestMissing");
                return;
            }

            if (blnDefaultedToEng)
                description =
                    LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing") +
                    Environment.NewLine + Environment.NewLine + description;
            
            DisplayDescription = description;
        }

        /// <summary>
        /// Loads all authors from an given loaded manifest.xml and saves them to _lstAuthors
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void GetManifestAuthors(XmlNode xmlDocument)
        {
            XmlNodeList xmlAuthorNodes = xmlDocument.SelectNodes("authors/author");

            if (xmlAuthorNodes == null)
                return;

            foreach (XmlNode objXmlNode in xmlAuthorNodes)
            {
                objXmlNode.TryGetField("name", out string authorName);
                objXmlNode.TryGetField("main", out bool authorMain);

                if(!string.IsNullOrEmpty(authorName))
                    //Maybe a stupid idea? But who would add two authors with the same name anyway?
                    AuthorDictionary.Add(authorName, authorMain);
            }
            //After the list is fully formed, set the display author
            SetDisplayAuthors();
        }

        /// <summary>
        /// Sets the DisplayAuthors to be a sting of all authors separated with "," and marked with main-
        /// </summary>
        private void SetDisplayAuthors()
        {
            List<string> authorsList = new List<string>();
            foreach ( KeyValuePair<string, bool> kvp in AuthorDictionary)
            {
                string formattedName = kvp.Key;

                if (kvp.Value)
                    formattedName = kvp.Key + LanguageManager.GetString("IsMainAuthor");

                authorsList.Add(formattedName);
            }
            DisplayAuthors = string.Join(", ", authorsList.ToArray());
        }

        /// <summary>
        /// Loads all dependencies from an given loaded manifest.xml and saves them to DependenciesList
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void GetDependencies(XmlNode xmlDocument)
        {
            XmlNodeList xmlDependencies = xmlDocument.SelectNodes("dependencies/dependency");

            if (xmlDependencies == null)
                return;

            foreach (XmlNode objXmlNode in xmlDependencies)
            {
                Guid depGuid = Guid.Empty;
                objXmlNode.TryGetField("name", out string newName);
                objXmlNode.TryGetField("version", out int newVersion);
                objXmlNode.TryGetGuidFieldQuickly("guid", ref depGuid);
                objXmlNode.TryGetField("isminimumversion", out bool isMinimumVersion);

                if (string.IsNullOrEmpty(newName) || newVersion == int.MinValue || depGuid == Guid.Empty)
                    continue;

                _lstDependencies.Add(Tuple.Create(depGuid, newName, newVersion, isMinimumVersion));
            }
        }

        /// <summary>
        /// Loads all dependencies from an given loaded manifest.xml and saves them to ExclusivitiesList
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void GetExclusivities(XmlNode xmlDocument)
        {
            XmlNodeList xmlExclusivities = xmlDocument.SelectNodes("exclusivities/exclusivity");

            if (xmlExclusivities == null)
                return;

            foreach (XmlNode objXmlNode in xmlExclusivities)
            {
                Guid newGuid = Guid.Empty;
                objXmlNode.TryGetField("name", out string newName);
                objXmlNode.TryGetField("version", out int newVersion);
                objXmlNode.TryGetGuidFieldQuickly("guid", ref newGuid);
                objXmlNode.TryGetField("ignoreversion", out bool isMinimumVersion);

                if (string.IsNullOrEmpty(newName) || newVersion == int.MinValue || newGuid == Guid.Empty)
                    continue;

                _lstExclusivities.Add(Tuple.Create(newGuid, newName, newVersion, isMinimumVersion));
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
        #endregion

        /// <summary>
        /// Loads and gets all relevant infos from the manifest.xml and stores them in the respective property
        /// </summary>
        private void Create()
        {
            if (_blnCreated)
                return;

            try
            {
                XmlDocument xmlObjManifest = new XmlDocument();
                string strFullDirectory = System.IO.Path.Combine(Path, "manifest.xml");

                //Hash = CalculateHash();

                if (!File.Exists(strFullDirectory))
                    return;

                xmlObjManifest.LoadStandard(strFullDirectory);

                var xmlNode = xmlObjManifest.SelectSingleNode("manifest");
                xmlNode.TryGetInt32FieldQuickly("version", ref _version);

                xmlNode.TryGetGuidFieldQuickly("guid", ref _guid);

                GetManifestDescriptions(xmlNode);
                GetManifestAuthors(xmlNode);
                GetDependencies(xmlNode);
                GetExclusivities(xmlNode);
            }
            catch (Exception ex)
            {
                Program.MainForm.ShowMessageBox(
                    string.Format(GlobalOptions.CultureInfo,
                        LanguageManager.GetString("Message_FailedLoad"), ex.Message),
                    string.Format(GlobalOptions.CultureInfo,
                        LanguageManager.GetString("MessageTitle_FailedLoad")
                        + " " + Name),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _blnCreated = true;
            }
        }
        #endregion

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the dependent upon directory
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public string CheckDependency(CharacterOptions objCharacterOptions)
        {
            var missingDependencyList = new List<string>();
            foreach (var (depGuid, depName, depVersion, isMinimumVersion) in DependenciesList)
            {
                if (isMinimumVersion)
                {
                    if (objCharacterOptions.EnabledCustomDataDirectoryInfos.All(loadedFile =>
                        depGuid != loadedFile.Guid && depVersion != loadedFile.Version))
                        missingDependencyList.Add(string.Format(depName + LanguageManager.GetString("AddMinimumVersion"), depVersion));
                }
                else
                {
                    if (objCharacterOptions.EnabledCustomDataDirectoryInfos.All(loadedFile =>
                        depGuid != loadedFile.Guid && depVersion >= loadedFile.Version))
                        missingDependencyList.Add(string.Format(depName + LanguageManager.GetString("AddVersion"), depVersion));
                }

            }
            var missingDependencies = string.Join(", ", missingDependencyList);
            return string.IsNullOrEmpty(missingDependencies) ? string.Empty : missingDependencies;
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the prohibited directories
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public string CheckExclusivity(CharacterOptions objCharacterOptions)
        {

            var existingProhibitedDirectory = new List<string>();

            foreach (var (forbiddenGuid, forbiddenName, forbiddenVersion, ignoreVersion) in ExclusivitiesList)
            {
                if (!ignoreVersion)
                {
                    if(objCharacterOptions.EnabledCustomDataDirectoryInfos.Any(enabledDirectory =>
                        enabledDirectory.Guid == forbiddenGuid && enabledDirectory.Version == forbiddenVersion))
                        existingProhibitedDirectory.Add(string.Format(forbiddenName + LanguageManager.GetString("AddVersion"), forbiddenVersion));
                }
                else
                {
                    if (objCharacterOptions.EnabledCustomDataDirectoryInfos.Any(enabledDirectory =>
                        enabledDirectory.Guid == forbiddenGuid))
                        existingProhibitedDirectory.Add(forbiddenName);
                }
            }

            var strExistingExclusivities = string.Join(", ", existingProhibitedDirectory);
            return string.IsNullOrEmpty(strExistingExclusivities) ? string.Empty : strExistingExclusivities;
        }

        /// <summary>
        /// Creates a string that displays which dependencies are missing or shouldn't be active to be displayed a tooltip.
        /// </summary>
        /// <param name="missingDependency">The string of all missing Dependencies</param>
        /// <param name="presentExclusivities">The string of all exclusivities that are active</param>
        /// <returns></returns>
        public static string BuildExclusivityDependencyString(string missingDependency = "", string presentExclusivities = "")
        {
            string formedString = string.Empty;

            if (!string.IsNullOrEmpty(missingDependency) && !string.IsNullOrEmpty(presentExclusivities))
            {
                formedString = string.Format(LanguageManager.GetString("Tooltip_Dependency_Missing"), missingDependency
                    + Environment.NewLine + Environment.NewLine
                    + string.Format(LanguageManager.GetString("Tooltip_Exclusivity_Present"), presentExclusivities));
            }
            else if (!string.IsNullOrEmpty(missingDependency))
            {
                formedString = string.Format(LanguageManager.GetString("Tooltip_Dependency_Missing"), missingDependency);
            }
            else if (!string.IsNullOrEmpty(presentExclusivities))
            {
                formedString = string.Format(LanguageManager.GetString("Tooltip_Exclusivity_Present"), presentExclusivities);
            }

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
        public int Version => _version;


        // /// <summary>
        // /// The Sha512 Hash of all non manifest.xml files in the directory
        // /// </summary>
        // public int Hash { get; private set; }

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(guid, str name, int version, bool isMinimumVersion).
        /// </summary>
        public IReadOnlyList<Tuple<Guid, string, int, bool>> DependenciesList => _lstDependencies;

        /// <summary>
        /// A list of all exclusivities each formatted as Tuple(int guid, str name, int version, bool ignoreVersion).
        /// </summary>
        public IReadOnlyList<Tuple<Guid, string, int, bool>> ExclusivitiesList => _lstExclusivities;

        /// <summary>
        /// A Dictionary containing all Descriptions, which uses the language code as key
        /// </summary>
        public IDictionary<string, string> DescriptionDictionary { get; } = new Dictionary<string, string>();

        /// <summary>
        /// The description to display
        /// </summary>
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// A Dictionary, that uses the author name as key and provides a bool if he is the main author
        /// </summary>
        public IDictionary<string, bool> AuthorDictionary { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// A string containing all Authors formatted as Author(main), Author2 
        /// </summary>
        public string DisplayAuthors { get; private set; }

        public Guid Guid => _guid;
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
            return new { Name, Path }.GetHashCode();
        }

        public bool Equals(CustomDataDirectoryInfo other)
        {
            return other != null && Name == other.Name && Path == other.Path;
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
    
}
