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
﻿using System;
using System.Collections.Generic;
using System.IO;
 using System.Linq;
 using System.Text;
using System.Xml;
using System.Windows.Forms;
 using System.Xml.Linq;

namespace Chummer
{
    // ReSharper disable InconsistentNaming
    public sealed class XmlManager
    {
        /// <summary>
        /// Used to cache XML files so that they do not need to be loaded and translated each time an object wants the file.
        /// </summary>
        private class XmlReference
        {
            /// <summary>
            /// Date/Time stamp on the XML file.
            /// </summary>
            public DateTime FileDate { get; set; }

            /// <summary>
            /// Name of the XML file.
            /// </summary>
            public string FileName { get; set; } = string.Empty;

            /// <summary>
            /// Whether or not the XML file has been successfully checked for duplicate guids.
            /// </summary>
            public bool DuplicatesChecked { get; set; }

            /// <summary>
            /// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public XmlDocument XmlContent { get; set; } = new XmlDocument();
        }

        private static readonly List<XmlReference> _lstXmlDocuments = new List<XmlReference>();

        #region Constructor and Instance
        static XmlManager()
        {
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
        }

        XmlManager()
        {
        }

        /// <summary>
        /// Glboal XmlManager instance.
        /// </summary>
        public static XmlManager Instance { get; } = new XmlManager();

        #endregion

        #region Methods
        /// <summary>
        /// Load the selected XML file and its associated custom file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        public XmlDocument Load(string strFileName)
        {
            string strPath = Path.Combine(Application.StartupPath, "data", strFileName);
            if (!File.Exists(strPath))
            {
                Utils.BreakIfDebug();
                return null;
            }
            DateTime datDate = File.GetLastWriteTime(strPath);

            // Look to see if this XmlDocument is already loaded.
            bool blnLoadFile = false;
            XmlReference objReference = _lstXmlDocuments.Find(x => x.FileName == strFileName);
            if (objReference == null)
            {
                // The file was not found in the reference list, so it must be loaded.
                objReference = new XmlReference();
                blnLoadFile = true;
                _lstXmlDocuments.Add(objReference);
            }
            // The file was found in the List, so check the last write time.
            else if (datDate != objReference.FileDate)
            {
                // The last write time does not match, so it must be reloaded.
                blnLoadFile = true;
            }

            // Create a new document that everything will be merged into.
            XmlDocument objDoc;
            XmlDocument objXmlFile = new XmlDocument();

            if (blnLoadFile)
            {
                objDoc = new XmlDocument();
                // write the root chummer node.
                XmlNode objCont = objDoc.CreateElement("chummer");
                objDoc.AppendChild(objCont);
                // Load the base file and retrieve all of the child nodes.
                objXmlFile.Load(strPath);
                XmlNodeList xmlNodeList = objXmlFile.SelectNodes("/chummer/*");
                if (xmlNodeList != null)
                    foreach (XmlNode objNode in xmlNodeList)
                    {
                        // Append the entire child node to the new document.
                        objDoc.DocumentElement.AppendChild(objDoc.ImportNode(objNode, true));
                    }

                // Load any override data files the user might have. Do not attempt this if we're loading the Improvements file.
                if (strFileName != "improvements.xml")
                {
                    strPath = Path.Combine(Application.StartupPath, "data");
                    foreach (string strFile in Directory.GetFiles(strPath, "override*_" + strFileName))
                    {
                        objXmlFile.Load(strFile);
                        foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                        {
                            foreach (XmlNode objType in objNode.ChildNodes)
                            {
                                if (objType["id"] != null)
                                {
                                    XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + "/" + objType.Name + "[id = \"" + objType["id"].InnerText.Replace("&amp;", "&") + "\"]");
                                    if (objItem != null)
                                        objItem.InnerXml = objType.InnerXml;
                                }
                                else if (objType["name"] != null)
                                {
                                    XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + "/" + objType.Name + "[name = \"" + objType["name"].InnerText.Replace("&amp;", "&") + "\"]");
                                    if (objItem != null)
                                        objItem.InnerXml = objType.InnerXml;
                                }
                            }
                        }
                    }

                    // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
                    foreach (string strFile in Directory.GetFiles(strPath, "custom*_" + strFileName))
                    {
                        objXmlFile.Load(strFile);
                        foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                        {
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>();
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                if (objChild.ParentNode != null)
                                {
                                    // Only do this if the child has the name or id field since this is what we must match on.
                                    if (objChild["id"] != null && null != objDoc.SelectSingleNode("/chummer/" + objChild.ParentNode.Name + "/" +
                                                                    objChild.Name + "[name = \"" +
                                                                    objChild["id"].InnerText + "\"]"))
                                    {
                                        lstDelete.Add(objChild);
                                    }
                                    else if (objChild["name"] != null && null != objDoc.SelectSingleNode("/chummer/" + objChild.ParentNode.Name + "/" +
                                                                    objChild.Name + "[name = \"" +
                                                                    objChild["name"].InnerText + "\"]"))
                                    {
                                        lstDelete.Add(objChild);
                                    }
                                }
                            }
                            // Remove the offending items from the node we're about to merge in.
                            foreach (XmlNode objRemoveNode in lstDelete)
                            {
                                objNode.RemoveChild(objRemoveNode);
                            }

                            // Append the entire child node to the new document.
                            objDoc.DocumentElement?.AppendChild(objDoc.ImportNode(objNode, true));
                        }
                    }
                }

                // Load the translation file for the current base data file if the selected language is not en-us.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    // Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
                    // The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
                    if (LanguageManager.Instance.DataDoc != null)
                    {
                        foreach (XmlNode objNode in LanguageManager.Instance.DataDoc.SelectNodes("/chummer/chummer[@file = \"" + strFileName + "\"]"))
                        {
                            foreach (XmlNode objType in objNode.ChildNodes)
                            {
                                foreach (XmlNode objChild in objType.ChildNodes)
                                {
                                    if (objChild["name"] != null)
                                    {
                                        // If this is a translatable item, find the proper node and add/update this information.
                                        XmlNode objItem =
                                            objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" +
                                                                    objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
                                        if (objItem != null)
                                        {
                                            if (objChild["translate"] != null)
                                            {
                                                objItem.InnerXml += "<translate>" + objChild["translate"].InnerXml + "</translate>";
                                            }
                                            if (objChild["page"] != null)
                                            {
                                                objItem.InnerXml += "<altpage>" + objChild["page"].InnerXml + "</altpage>";
                                            }
                                            if (objChild["code"] != null)
                                            {
                                                objItem.InnerXml += "<altcode>" + objChild["code"].InnerXml + "</altcode>";
                                            }
                                            if (objChild["advantage"] != null)
                                            {
                                                objItem.InnerXml += "<altadvantage>" + objChild["advantage"].InnerXml + "</altadvantage>";
                                            }
                                            if (objChild["disadvantage"] != null)
                                            {
                                                objItem.InnerXml += "<altdisadvantage>" + objChild["disadvantage"].InnerXml + "</altdisadvantage>";
                                            }
                                            if (objChild.Attributes?["translate"] != null)
                                            {
                                                // Handle Category name translations.
                                                (objItem as XmlElement)?.SetAttribute("translate", objChild.Attributes["translate"].InnerXml);
                                            }

                                            // Check for Skill Specialization information.
                                            if (strFileName == "skills.xml")
                                            {
                                                if (objChild["specs"] != null)
                                                {
                                                    foreach (XmlNode objSpec in objChild.SelectNodes("specs/spec"))
                                                    {
                                                        if (objSpec.Attributes?["translate"] != null)
                                                        {
                                                            XmlElement objSpecItem = objItem.SelectSingleNode("specs/spec[. = \"" + objSpec.InnerXml + "\"]") as XmlElement;
                                                            objSpecItem?.SetAttribute("translate", objSpec.Attributes["translate"].InnerXml);
                                                        }
                                                    }
                                                }
                                            }
                                            // Check for Metavariant information.
                                            else if (strFileName == "metatypes.xml")
                                            {
                                                if (objChild["metavariants"] != null)
                                                {
                                                    foreach (XmlNode objMetavariant in objChild.SelectNodes("metavariants/metavariant"))
                                                    {
                                                        if (objMetavariant["name"] != null && objChild["name"] != null)
                                                        {
                                                            XmlNode objMetavariantItem =
                                                                objDoc.SelectSingleNode(
                                                                    "/chummer/metatypes/metatype[name = \"" +
                                                                    objChild["name"].InnerXml +
                                                                    "\"]/metavariants/metavariant[name = \"" +
                                                                    objMetavariant["name"].InnerXml + "\"]");
                                                            if (objMetavariantItem != null)
                                                            {
                                                                if (objMetavariant["translate"] != null)
                                                                {
                                                                    objMetavariantItem.InnerXml += "<translate>" +
                                                                                                   objMetavariant[
                                                                                                       "translate"].InnerXml +
                                                                                                   "</translate>";
                                                                }
                                                                if (objMetavariant["page"] != null)
                                                                {
                                                                    objMetavariantItem.InnerXml += "<altpage>" +
                                                                                                   objMetavariant["page"]
                                                                                                       .InnerXml +
                                                                                                   "</altpage>";
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            // Check for Martial Art Advantage information.
                                            else if (strFileName == "martialarts.xml")
                                            {
                                                if (objChild["advantages"] != null)
                                                {
                                                    foreach (XmlNode objAdvantage in objChild.SelectNodes("techniques/technique"))
                                                    {
                                                        if (objAdvantage.Attributes?["translate"] != null)
                                                        {
                                                            XmlElement objAdvantageItem = objItem.SelectSingleNode("techniques/technique[. = \"" + objAdvantage.InnerXml + "\"]") as XmlElement;
                                                            objAdvantageItem?.SetAttribute("translate", objAdvantage.Attributes["translate"].InnerXml);
                                                        }
                                                    }
                                                }
                                            }
                                            // Check for Mentor Spirit/Paragon choice information.
                                            else if (strFileName == "mentors.xml" || strFileName == "paragons.xml")
                                            {
                                                if (objChild["choices"] != null)
                                                {
                                                    foreach (XmlNode objChoice in objChild.SelectNodes("choices/choice"))
                                                    {
                                                        if (objChoice["name"] != null && objChoice["translate"] != null)
                                                        {
                                                            XmlNode objChoiceItem = objItem.SelectSingleNode("choices/choice[name = \"" + objChoice["name"].InnerXml + "\"]");
                                                            if (objChoiceItem != null)
                                                                objChoiceItem.InnerXml += "<translate>" + objChoice["translate"].InnerXml + "</translate>";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (objChild.Attributes?["translate"] != null)
                                    {
                                        // Handle Category name translations.
                                        XmlElement objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[. = \"" + objChild.InnerXml.Replace("&amp;", "&") + "\"]") as XmlElement;
                                        // Expected result is null if not found.
                                        objItem?.SetAttribute("translate", objChild.Attributes["translate"].InnerXml);
                                    }
                                }
                            }
                        }
                    }
                }

                // Cache the merged document and its relevant information.
                objReference.FileDate = datDate;
                objReference.FileName = strFileName;
                if (GlobalOptions.Instance.LiveCustomData)
                    objReference.XmlContent = objDoc.Clone() as XmlDocument;
                else
                    objReference.XmlContent = objDoc;
            }
            else
            {
                // A new XmlDocument is created by loading the a copy of the cached one so that we don't stuff custom content into the cached copy
                // (which we don't want and also results in multiple copies of each custom item).
                // Pull the document from cache.
                if (GlobalOptions.Instance.LiveCustomData)
                    objDoc = objReference.XmlContent.Clone() as XmlDocument;
                else
                    objDoc = objReference.XmlContent;
            }

            // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
            if (GlobalOptions.Instance.LiveCustomData && objDoc != null && strFileName != "improvements.xml")
            {
                strPath = Path.Combine(Application.StartupPath, "customdata");
                if (Directory.Exists(strPath))
                {
                    foreach (string strFile in Directory.GetFiles(strPath, "custom*_" + strFileName))
                    {
                        objXmlFile.Load(strFile);
                        foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                        {
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>();
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                // Only do this if the child has the name or id field since this is what we must match on.
                                if (objChild["id"] != null &&
                                    null != objDoc.SelectSingleNode("/chummer/" + objChild.ParentNode.Name + "/" +
                                                                    objChild.Name + "[name = \"" +
                                                                    objChild["id"].InnerText + "\"]"))
                                {
                                    lstDelete.Add(objChild);
                                }
                                else if (objChild["name"] != null &&
                                         null != objDoc.SelectSingleNode("/chummer/" + objChild.ParentNode.Name + "/" +
                                                                         objChild.Name + "[name = \"" +
                                                                         objChild["name"].InnerText + "\"]"))
                                {
                                    lstDelete.Add(objChild);
                                }
                            }
                            // Remove the offending items from the node we're about to merge in.
                            foreach (XmlNode objRemoveNode in lstDelete)
                            {
                                objNode.RemoveChild(objRemoveNode);
                            }

                            // Append the entire child node to the new document.
                            objDoc.DocumentElement?.AppendChild(objDoc.ImportNode(objNode, true));
                        }
                    }
                }
            }

            //Check for non-unique guids in the loaded XML file. Ignore improvements.xml since the ids are used in a different way.
            if (strFileName == "improvements.xml" || objReference.DuplicatesChecked) return objDoc;
            {
                foreach (XmlNode objNode in objDoc.SelectNodes("/chummer/*"))
                {
                    //Ignore the version node, if present. 
                    if (objNode.Name == "version" || !objNode.HasChildNodes) continue;
                    //Parse the node into an XDocument for LINQ parsing. 
                    XDocument y = XDocument.Parse(objNode.OuterXml);
                    string strNode = (from XmlNode o in objNode.ChildNodes where o.NodeType != XmlNodeType.Comment select o.Name).FirstOrDefault();

                    //Grab the first XML node that isn't a comment. 
                    if (strNode == null) continue;
                    var duplicatesList = y.Descendants(strNode)
                        .GroupBy(g => (string) g.Element("id") ?? "")
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();
                    int i = duplicatesList.Count(o => !string.IsNullOrWhiteSpace(o));
                    if (i <= 0)
                    {
                        objReference.DuplicatesChecked = true;
                        continue;
                    }
                    string duplicates = string.Join("\n", duplicatesList);
                    MessageBox.Show(
                        LanguageManager.Instance.GetString("Message_DuplicateGuidWarning")
                            .Replace("{0}", i.ToString())
                            .Replace("{1}", strFileName)
                            .Replace("{2}", duplicates));
                }
            }

            return objDoc;
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        public void Verify(string strLanguage, List<string> lstBooks)
        {
            XmlDocument objLanguageDoc = new XmlDocument();
            string languageDirectoryPath = Path.Combine(Application.StartupPath, "lang");
            string strFilePath = Path.Combine(languageDirectoryPath, strLanguage + "_data.xml");
            objLanguageDoc.Load(strFilePath);

            string strLangPath = Path.Combine(languageDirectoryPath, "results_" + strLanguage + ".xml");
            FileStream objStream = new FileStream(strLangPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode);
            objWriter.Formatting = Formatting.Indented;
            objWriter.Indentation = 1;
            objWriter.IndentChar = '\t';

            objWriter.WriteStartDocument();
            // <results>
            objWriter.WriteStartElement("results");

            string strPath = Path.Combine(Application.StartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strPath, "*.xml"))
            {
                string strFileName = Path.GetFileName(strFile);

                // Do not bother to check custom files.
                if (!string.IsNullOrEmpty(strFileName) && !strFileName.StartsWith("custom") && !strFile.StartsWith("override") && !strFile.Contains("packs.xml") && !strFile.Contains("ranges.xml"))
                {
                    // Load the current English file.
                    XmlDocument objEnglishDoc = Load(strFileName);
                    XmlNode objEnglishRoot = objEnglishDoc.SelectSingleNode("/chummer");

                    // First pass: make sure the document exists.
                    bool blnExists = false;
                    XmlNode objLanguageRoot = objLanguageDoc.SelectSingleNode("/chummer/chummer[@file = \"" + strFileName + "\"]");
                    if (objLanguageRoot != null)
                        blnExists = true;

                    // <file name="x" exists="y">
                    objWriter.WriteStartElement("file");
                    objWriter.WriteAttributeString("name", strFileName);
                    objWriter.WriteAttributeString("exists", blnExists.ToString());

                    if (blnExists)
                    {
                        foreach (XmlNode objType in objEnglishRoot.ChildNodes)
                        {
                            objWriter.WriteStartElement(objType.Name);
                            foreach (XmlNode objChild in objType.ChildNodes)
                            {
                                // If the Node has a source element, check it and see if it's in the list of books that were specified.
                                // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                                bool blnContinue = false;
                                if (objChild["source"] != null)
                                {
                                    foreach (string strBook in lstBooks)
                                    {
                                        if (strBook == objChild["source"].InnerText)
                                        {
                                            blnContinue = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                    blnContinue = true;

                                if (blnContinue)
                                {
                                    if (objType.Name != "version" && !((objType.Name == "costs" || objType.Name == "safehousecosts") && strFile.EndsWith("lifestyles.xml")))
                                    {
                                        // Look for a matching entry in the Language file.
                                        if (objChild["name"] != null)
                                        {
                                            XmlNode objNode = objLanguageRoot.SelectSingleNode(objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerText + "\"]");
                                            if (objNode != null)
                                            {
                                                // A match was found, so see what elements, if any, are missing.
                                                bool blnTranslate = false;
                                                bool blnAltPage = false;
                                                bool blnAdvantage = false;
                                                bool blnDisadvantage = false;

                                                if (objChild.HasChildNodes)
                                                {
                                                    if (objNode["translate"] != null)
                                                        blnTranslate = true;

                                                    // Do not mark page as missing if the original does not have it.
                                                    if (objChild["page"] != null)
                                                    {
                                                        if (objNode["page"] != null)
                                                            blnAltPage = true;
                                                    }
                                                    else
                                                        blnAltPage = true;

                                                    if (strFile.EndsWith("mentors.xml") || strFile.EndsWith("paragons.xml"))
                                                    {
                                                        if (objNode["advantage"] != null)
                                                            blnAdvantage = true;
                                                        if (objNode["disadvantage"] != null)
                                                            blnDisadvantage = true;
                                                    }
                                                    else
                                                    {
                                                        blnAdvantage = true;
                                                        blnDisadvantage = true;
                                                    }
                                                }
                                                else
                                                {
                                                    blnAltPage = true;
                                                    if (objNode.Attributes?["translate"] != null)
                                                        blnTranslate = true;
                                                }

                                                // At least one pice of data was missing so write out the result node.
                                                if (!blnTranslate || !blnAltPage || !blnAdvantage || !blnDisadvantage)
                                                {
                                                    // <results>
                                                    objWriter.WriteStartElement(objChild.Name);
                                                    objWriter.WriteAttributeString("exists", "True");
                                                    objWriter.WriteElementString("name", objChild["name"].InnerText);
                                                    if (!blnTranslate)
                                                        objWriter.WriteElementString("missing", "translate");
                                                    if (!blnAltPage)
                                                        objWriter.WriteElementString("missing", "page");
                                                    if (!blnAdvantage)
                                                        objWriter.WriteElementString("missing", "advantage");
                                                    if (!blnDisadvantage)
                                                        objWriter.WriteElementString("missing", "disadvantage");
                                                    // </results>
                                                    objWriter.WriteEndElement();
                                                }
                                            }
                                            else
                                            {
                                                // No match was found, so write out that the data item is missing.
                                                // <result>
                                                objWriter.WriteStartElement(objChild.Name);
                                                objWriter.WriteAttributeString("exists", "False");
                                                objWriter.WriteElementString("name", objChild["name"].InnerText);
                                                // </result>
                                                objWriter.WriteEndElement();
                                            }

                                            if (strFileName == "metatypes.xml")
                                            {
                                                if (objChild["metavariants"] != null)
                                                {
                                                    foreach (XmlNode objMetavariant in objChild.SelectNodes("metavariants/metavariant"))
                                                    {
                                                        XmlNode objTranslate = objLanguageRoot.SelectSingleNode("metatypes/metatype[name = \"" + objChild["name"].InnerText + "\"]/metavariants/metavariant[name = \"" + objMetavariant["name"].InnerText + "\"]");
                                                        if (objTranslate != null)
                                                        {
                                                            bool blnTranslate = false;
                                                            bool blnAltPage = false;

                                                            if (objTranslate["translate"] != null)
                                                                blnTranslate = true;
                                                            if (objTranslate["page"] != null)
                                                                blnAltPage = true;

                                                            // Item exists, so make sure it has its translate attribute populated.
                                                            if (!blnTranslate || !blnAltPage)
                                                            {
                                                                // <result>
                                                                objWriter.WriteStartElement("metavariants");
                                                                objWriter.WriteStartElement("metavariant");
                                                                objWriter.WriteAttributeString("exists", "True");
                                                                objWriter.WriteElementString("name", objMetavariant["name"].InnerText);
                                                                if (!blnTranslate)
                                                                    objWriter.WriteElementString("missing", "translate");
                                                                if (!blnAltPage)
                                                                    objWriter.WriteElementString("missing", "page");
                                                                objWriter.WriteEndElement();
                                                                // </result>
                                                                objWriter.WriteEndElement();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // <result>
                                                            objWriter.WriteStartElement("metavariants");
                                                            objWriter.WriteStartElement("metavariant");
                                                            objWriter.WriteAttributeString("exists", "False");
                                                            objWriter.WriteElementString("name", objMetavariant.InnerText);
                                                            objWriter.WriteEndElement();
                                                            // </result>
                                                            objWriter.WriteEndElement();
                                                        }
                                                    }
                                                }
                                            }

                                            if (strFile == "martialarts.xml")
                                            {
                                                if (objChild["advantages"] != null)
                                                {
                                                    foreach (XmlNode objAdvantage in objChild.SelectNodes("techniques/technique"))
                                                    {
                                                        XmlNode objTranslate = objLanguageRoot.SelectSingleNode("martialarts/martialart[name = \"" + objChild["name"].InnerText + "\"]/techniques/technique[. = \"" + objAdvantage.InnerText + "\"]");
                                                        if (objTranslate != null)
                                                        {
                                                            // Item exists, so make sure it has its translate attribute populated.
                                                            if (objTranslate.Attributes?["translate"] == null)
                                                            {
                                                                // <result>
                                                                objWriter.WriteStartElement("martialarts");
                                                                objWriter.WriteStartElement("advantage");
                                                                objWriter.WriteAttributeString("exists", "True");
                                                                objWriter.WriteElementString("name", objAdvantage.InnerText);
                                                                objWriter.WriteElementString("missing", "translate");
                                                                objWriter.WriteEndElement();
                                                                // </result>
                                                                objWriter.WriteEndElement();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // <result>
                                                            objWriter.WriteStartElement("martialarts");
                                                            objWriter.WriteStartElement("advantage");
                                                            objWriter.WriteAttributeString("exists", "False");
                                                            objWriter.WriteElementString("name", objAdvantage.InnerText);
                                                            objWriter.WriteEndElement();
                                                            // </result>
                                                            objWriter.WriteEndElement();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (objChild.Name == "#comment")
                                        {
                                            //Ignore this node, as it's a comment node.
                                        }
                                        else if (!string.IsNullOrEmpty(objChild.InnerText))
                                        {
                                            // The item does not have a name which means it should have a translate CharacterAttribute instead.
                                            XmlNode objNode =
                                                objLanguageRoot.SelectSingleNode(objType.Name + "/" + objChild.Name + "[. = \"" + objChild.InnerText + "\"]");
                                            if (objNode != null)
                                            {
                                                // Make sure the translate attribute is populated.
                                                if (objNode.Attributes?["translate"] == null)
                                                {
                                                    // <result>
                                                    objWriter.WriteStartElement(objChild.Name);
                                                    objWriter.WriteAttributeString("exists", "True");
                                                    objWriter.WriteElementString("name", objChild.InnerText);
                                                    objWriter.WriteElementString("missing", "translate");
                                                    // </result>
                                                    objWriter.WriteEndElement();
                                                }
                                            }
                                            else
                                            {
                                                // No match was found, so write out that the data item is missing.
                                                // <result>
                                                objWriter.WriteStartElement(objChild.Name);
                                                objWriter.WriteAttributeString("exists", "False");
                                                objWriter.WriteElementString("name", objChild.InnerText);
                                                // </result>
                                                objWriter.WriteEndElement();
                                            }
                                        }
                                    }
                                }
                            }
                            objWriter.WriteEndElement();
                        }

                        // Now loop through the translation file and determine if there are any entries in there that are not part of the base content.
                        foreach (XmlNode objType in objLanguageRoot.ChildNodes)
                        {
                            foreach (XmlNode objChild in objType.ChildNodes)
                            {
                                // Look for a matching entry in the English file.
                                if (objChild["name"] != null)
                                {
                                    XmlNode objNode = objEnglishRoot.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerText + "\"]");
                                    if (objNode == null)
                                    {
                                        // <noentry>
                                        objWriter.WriteStartElement("noentry");
                                        objWriter.WriteStartElement(objChild.Name);
                                        objWriter.WriteElementString("name", objChild["name"].InnerText);
                                        objWriter.WriteEndElement();
                                        // </noentry>
                                        objWriter.WriteEndElement();
                                    }
                                }
                            }
                        }
                    }

                    // </file>
                    objWriter.WriteEndElement();
                }
            }

            // </results>
            objWriter.WriteEndElement();
            objWriter.WriteEndDocument();
            objWriter.Close();
            objStream.Close();
        }
        #endregion
    }
}