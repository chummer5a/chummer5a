using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Classes
{
    /// <summary>
    /// This class is invoked by reflection to provide a various file system related lists that options requires
    /// </summary>
    class OptionsFileSystemHelper
    {

        public static List<ListItem<string>> GetListOfCharacterSheets()
        {
            string sheetsDirectoryPath = Path.Combine(Application.StartupPath, "sheets");
            if(!Directory.Exists(sheetsDirectoryPath)) return new List<ListItem<string>>(0);

            List<ListItem<string>> names = Directory.GetFiles(sheetsDirectoryPath)
                    .Where(s => s.EndsWith(".xsl"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Select(name => new ListItem<string>(name, name))
                    .ToList();

            if (GlobalOptions.Language != "en-us")
            {
                string langSheets = Path.Combine(sheetsDirectoryPath, GlobalOptions.Language);
                if (Directory.Exists(langSheets))
                {
                    string langfolder = LanguageManager.DictionaryLanguages[GlobalOptions.Language].LanguageName;

                    names.AddRange(
                        Directory.GetFileSystemEntries(langSheets)
                        .Where(s => s.EndsWith(".xsl"))
                        .Select(Path.GetFileNameWithoutExtension)
                        .Select(name => new ListItem<string>(langfolder + ":" + name, name))
                        );
                }
            }

            return names;
        }

        public static List<ListItem<string>> GetListOfLanguages()
        {
            List<ListItem<string>> lstLanguages = new List<ListItem<string>>();
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");

            foreach (string filePath in languageFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument();

                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                    {
                        xmlDocument.Load(objStreamReader);
                    }
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/chummer/name");
                if (node == null)
                    continue;

                lstLanguages.Add(new ListItem(Path.GetFileNameWithoutExtension(filePath), node.InnerText));
            }

            return lstLanguages;
        }
    }
}
