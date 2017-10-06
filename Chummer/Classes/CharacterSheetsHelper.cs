using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Classes
{
    class CharacterSheetsHelper
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

            if (GlobalOptions.Instance.Language != "en-us")
            {
                string langSheets = Path.Combine(sheetsDirectoryPath, GlobalOptions.Instance.Language);
                if (Directory.Exists(langSheets))
                {
                    XmlDocument objLanguageDocument = LanguageManager.Instance.XmlDoc;
                    string langfolder = objLanguageDocument.SelectSingleNode("/chummer/name").InnerText;

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
    }
}
