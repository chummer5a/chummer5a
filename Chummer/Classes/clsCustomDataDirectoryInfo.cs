using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    public class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>
    {
        private readonly List<Tuple<string, bool>> _lstAuthors = new List<Tuple<string, bool>>();
        private bool _blnCreated;

        private string _strVersion;

        private static readonly Logger s_Logger = LogManager.GetCurrentClassLogger();

        private bool _blnIsSaving;

        //Tuple<int Hash, string Version, string Name>
        private readonly List<Tuple<int, string, string>> _lstDependencies= new List<Tuple<int, string, string>>();
        private readonly List<Tuple<int, string, string>> _lstExclusivities = new List<Tuple<int, string, string>>();

        public CustomDataDirectoryInfo(string strName, string strPath)
        {
            Name = strName;
            Path = strPath;
            Create();
        }

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
                }

                return intReturn;
            }

            return string.Compare(Name, obj.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

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

        #region Create and Load Methods

        /// <summary>
        /// Selects the description in the correct language and saves it to _strDisplayDescription
        /// </summary>
        private void GetDisplayDescription()
        {
            bool blnDefaultedToEng = false;
            var objDescription = DescriptionList.Find(description => description.Item2.Equals(GlobalOptions.Language));

            if (objDescription == null)
            {
                objDescription = DescriptionList.Find(description => description.Item2.Equals("en-us"));
                blnDefaultedToEng = true;
            }

            if(objDescription == null)
                return;

            string strDescription = objDescription.Item1;

            if (blnDefaultedToEng)
                strDescription =
                    LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing") +
                    Environment.NewLine + Environment.NewLine + strDescription;

            DisplayDescription = strDescription;
        }

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

                Tuple<string, string> newDescriptionTuple = new Tuple<string, string>(text, language);
                DescriptionList.Add(newDescriptionTuple);
            }
            GetDisplayDescription();
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
                objXmlNode.TryGetField("name", out bool authorMain);

                Tuple<string, bool> newAuthor = new Tuple<string, bool>(authorName, authorMain);
                _lstAuthors.Add(newAuthor);
            }
            //After the list is fully formed, set the display author
            GetDisplayAuthors();
        }

        /// <summary>
        /// Sets the DisplayAuthors to be a sting of all authors separated with "," and marked with main-
        /// </summary>
        private void GetDisplayAuthors()
        {
            List<string> authorsList = new List<string>();
            foreach (var (authorName, authorIsMain) in _lstAuthors)
            {
                string formattedName = authorName;

                if (authorIsMain)
                    formattedName = authorName + "(" + LanguageManager.GetString("Main_Author") + ")";

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
                objXmlNode.TryGetField("name", out string newName);
                objXmlNode.TryGetField("version", out string newVersion);
                objXmlNode.TryGetField("hash", out int newHash);

                if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newVersion) || newHash == int.MinValue)
                    continue;

                _lstDependencies.Add(Tuple.Create(newHash, newName, newVersion));
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
                objXmlNode.TryGetField("name", out string newName);
                objXmlNode.TryGetField("version", out string newVersion);
                objXmlNode.TryGetField("hash", out int newHash);

                if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newVersion) || newHash == int.MinValue)
                    continue;

                _lstExclusivities.Add(Tuple.Create(newHash, newName, newVersion));
            }
        }

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
        }

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

                Hash = CalculateHash();

                if (!File.Exists(strFullDirectory))
                    return;

                xmlObjManifest.LoadStandard(strFullDirectory);

                var xmlNode = xmlObjManifest.SelectSingleNode("manifest");
                xmlNode.TryGetStringFieldQuickly("version", ref _strVersion);

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
        /// Checks if all necessary dependencies are loaded in the passed Character Options
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public string CheckDependency(CharacterOptions objCharacterOptions)
        {
            var strMissingDependencies = string.Join(", ", (from dependency in DependenciesList let intNeededHash = dependency.Item1
                where objCharacterOptions.EnabledCustomDataDirectoryInfos.All(obj => obj.Hash != intNeededHash)
                    select string.Format(dependency.Item2 + " (" + dependency.Item3 + ")")).ToArray());

            return string.IsNullOrEmpty(strMissingDependencies)? string.Empty : strMissingDependencies;
        }

        /// <summary>
        /// Checks if any prohibited custom data directories are activated
        /// </summary>
        /// <param name="objCharacterOptions"></param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public string CheckExclusivity(CharacterOptions objCharacterOptions)
        {
            var strExistingExclusivities = string.Join(", ", (from dependency in ExclusivitiesList let intProhibitedHash = dependency.Item1
                where objCharacterOptions.EnabledCustomDataDirectoryInfos.Any(obj => obj.Hash == intProhibitedHash)
                    select string.Format(dependency.Item2 + " (" + dependency.Item3 + ")")).ToArray());

            return string.IsNullOrEmpty(strExistingExclusivities) ? string.Empty : strExistingExclusivities;
        }

        public bool Save()
        {
            if (_blnIsSaving)
                return false;

            string strFileName = "manifest";
            
            bool blnErrorFree = true;
            _blnIsSaving = true;
            using (MemoryStream objStream = new MemoryStream())
            {
                using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                })
                {
                    objWriter.WriteStartDocument();

                    //<manifest>
                    objWriter.WriteStartElement("manifest");

                    objWriter.WriteElementString("name",Name);
                    objWriter.WriteElementString("version", Version);

                    //<authors>
                    objWriter.WriteStartElement("authors");
                    foreach (var (name, isMain) in AuthorList)
                    {
                        objWriter.WriteElementString("name", name);
                        objWriter.WriteElementString("main", isMain.ToString());
                    }
                    //</authors>
                    objWriter.WriteEndElement();

                    //<descriptions>
                    objWriter.WriteStartElement("descriptions");
                    foreach (var (name, languageCode) in DescriptionList)
                    {
                        objWriter.WriteStartElement("descriptions");
                        objWriter.WriteElementString("name", name);
                        objWriter.WriteElementString("lang", languageCode);
                        objWriter.WriteEndElement();
                    }
                    //</descriptions>
                    objWriter.WriteEndElement();

                    //<dependencies>
                    objWriter.WriteStartElement("dependencies");
                    foreach (var (hash, name, version) in DependenciesList)
                    {
                        objWriter.WriteStartElement("dependency");
                        objWriter.WriteElementString("hash", hash.ToString());
                        objWriter.WriteElementString("name", name);
                        objWriter.WriteElementString("version", version);
                        objWriter.WriteEndElement();
                    }
                    //</dependencies>
                    objWriter.WriteEndElement();

                    //<exclusivities>
                    objWriter.WriteStartElement("exclusivities");
                    foreach (var (hash, name, version) in ExclusivitiesList)
                    {
                        objWriter.WriteStartElement("exclusivity");
                        objWriter.WriteElementString("hash", hash.ToString());
                        objWriter.WriteElementString("name", name);
                        objWriter.WriteElementString("version", version);
                        objWriter.WriteEndElement();
                    }
                    //</exclusivities>
                    objWriter.WriteEndElement();

                    //</manifest>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                    objWriter.Flush();
                    objStream.Position = 0;

                    try
                    {
                        XmlDocument objDoc = new XmlDocument { XmlResolver = null };
                        using (XmlReader objXmlReader = XmlReader.Create(objStream, GlobalOptions.SafeXmlReaderSettings))
                            objDoc.Load(objXmlReader);
                        objDoc.Save(strFileName);
                    }
                    catch (IOException e)
                    {
                        s_Logger.Error(e);
                        if (Utils.IsUnitTest)
                            throw;
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                    catch (XmlException ex)
                    {
                        s_Logger.Warn(ex);
                        if (Utils.IsUnitTest)
                            throw;
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (Utils.IsUnitTest)
                            throw;
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                }
            }
            return blnErrorFree;
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

        #region Properties

        public string Name { get; }

        public string Path { get; }

        /// <summary>
        /// The description to display
        /// </summary>
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// The version of the custom data directory
        /// </summary>
        public string Version => _strVersion;

        /// <summary>
        /// A string containing all Authors formatted as Author(main), Author2 
        /// </summary>
        public string DisplayAuthors { get; private set; }

        /// <summary>
        /// The Sha512 Hash of all non manifest.xml files in the directory
        /// </summary>
        public int Hash { get; private set; }

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(int Hash, str name, str version).
        /// </summary>
        public IReadOnlyList<Tuple<int, string, string>> DependenciesList => _lstDependencies;

        /// <summary>
        /// A list of all exclusivities each formatted as Tuple(int Hash, str name, str version).
        /// </summary>
        public IReadOnlyList<Tuple<int, string, string>> ExclusivitiesList => _lstExclusivities;

        /// <summary>
        /// A list of all descriptions each formatted as Tuple(str Description, str LanguageCode)
        /// </summary>
        public List<Tuple<string, string>> DescriptionList { get; } = new List<Tuple<string, string>>();

        public IReadOnlyList<Tuple<string, bool>> AuthorList => _lstAuthors;
    }
    #endregion
}
