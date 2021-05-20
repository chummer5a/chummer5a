using System;
using System.IO;
using System.Xml;

namespace Chummer
{
    public class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>
    {
        private string _strManifestDescription;

        public CustomDataDirectoryInfo(string strName, string strPath)
        {
            Name = strName;
            Path = strPath;
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

        /// <summary>
        /// Reads in an Manifest.xml in the given directory and returns it's language specific description. Defaults to an description without any attributes.
        /// </summary>
        /// <returns>A description as String, adds a comment if it defaulted to a node without language attribute</returns>
        private string GetManifestDescription()
        {
            //TODO: As soon, as we got more stuff to read from the manifest. Rewrite this to load the file once and read everything we need from it.
            string strLanguage = GlobalOptions.Language;
            XmlDocument xmlObjManifest = new XmlDocument();
            string strFullDirectory = System.IO.Path.Combine(Path, "manifest.xml");
            bool blnDefaultedToEng = false;

            //This will come up very often, so check this here instead of handling the exception. It also catches all exceptions regarding permission and bad strings(Paths)
            if (!File.Exists(strFullDirectory))
                return string.Empty;
            try
            {
                xmlObjManifest.LoadStandard(strFullDirectory);
            }
            //The .xml in malformed or not really an .xml
            catch (XmlException ex)
            {
                return string.Format(GlobalOptions.CultureInfo,
                    LanguageManager.GetString("Message_FailedLoad"),
                    ex.Message);
            }
            catch (NotSupportedException ex)
            {
                return string.Format(GlobalOptions.CultureInfo,
                    LanguageManager.GetString("Message_FailedLoad"),
                    ex.Message);
            }
            //Try to get a language specific description
            XmlNode xmlNodeDescription = xmlObjManifest.SelectSingleNode("manifest/descriptions/description[@lang='" + strLanguage + "']");

            if (xmlNodeDescription == null)
            {
                //select a node without any attribute, this should be the English one
                xmlNodeDescription = xmlObjManifest.SelectSingleNode("manifest/descriptions/description[count(@*)=0]");

                //If we are in English we will always "default" to the string without an Attribute... which should be the English one. This way we don't add the error to each description
                if (!strLanguage.Equals("en-us"))
                    blnDefaultedToEng = true;
            }

            //If neither a lang specific, nor an default en-us string could be found
            if (xmlNodeDescription == null)
                return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestDescriptionMissing");

            string strDescription = xmlNodeDescription.InnerText;

            //Add that the correct language could not be found
            if (blnDefaultedToEng)
                strDescription =
                    LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing") +
                    Environment.NewLine + Environment.NewLine + strDescription;

            return string.IsNullOrWhiteSpace(strDescription) ? string.Empty : strDescription;
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

        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(_strManifestDescription))
                    _strManifestDescription = GetManifestDescription();
                return _strManifestDescription;
            }
        }

        #endregion
    }
}
