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

        private static readonly HashSet<XmlReference> _lstXmlDocuments = new HashSet<XmlReference>();
        private static readonly List<string> _lstDataDirectories = new List<string>();

        #region Constructor
        static XmlManager()
        {
            LanguageManager.Load(GlobalOptions.Language, null);
            _lstDataDirectories.Add(Path.Combine(Application.StartupPath, "data"));
            foreach (CustomDataDirectoryInfo objCustomDataDirectory in GlobalOptions.CustomDataDirectoryInfo.Where(x => x.Enabled))
            {
                _lstDataDirectories.Add(objCustomDataDirectory.Path);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load the selected XML file and its associated custom file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        public static XmlDocument Load(string strFileName, bool blnLoadFile = false)
        {
            bool blnFileFound = false;
            string strPath = string.Empty;
            foreach (string strDirectory in _lstDataDirectories)
            {
                strPath = Path.Combine(strDirectory, strFileName);
                if (File.Exists(strPath))
                {
                    blnFileFound = true;
                    break;
                }
            }
            if (!blnFileFound)
            {
                Utils.BreakIfDebug();
                return null;
            }
            DateTime datDate = File.GetLastWriteTime(strPath);

            // Look to see if this XmlDocument is already loaded.
            XmlReference objReference = _lstXmlDocuments.FirstOrDefault(x => x.FileName == strFileName);
            if (objReference == null || blnLoadFile)
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
                XmlElement objDocElement = objDoc.DocumentElement;
                // Load the base file and retrieve all of the child nodes.
                objXmlFile.Load(strPath);
                XmlNodeList xmlNodeList = objXmlFile.SelectNodes("/chummer/*");
                if (xmlNodeList != null)
                    foreach (XmlNode objNode in xmlNodeList)
                    {
                        // Append the entire child node to the new document.
                        objDocElement.AppendChild(objDoc.ImportNode(objNode, true));
                    }

                // Load any override data files the user might have. Do not attempt this if we're loading the Improvements file.
                if (strFileName != "improvements.xml")
                {
                    foreach (string strLoopPath in _lstDataDirectories)
                    {
                        foreach (string strFile in Directory.GetFiles(strLoopPath, "override*_" + strFileName))
                        {
                            objXmlFile.Load(strFile);
                            foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                            {
                                foreach (XmlNode objType in objNode.ChildNodes)
                                {
                                    string strFilter = string.Empty;
                                    if (objType["id"] != null)
                                        strFilter = "id = \"" + objType["id"].InnerText.Replace("&amp;", "&") + "\"";
                                    if (objType["name"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(strFilter))
                                            strFilter += " and ";
                                        strFilter += "name = \"" + objType["name"].InnerText.Replace("&amp;", "&") + "\"";
                                    }
                                    if (!string.IsNullOrEmpty(strFilter))
                                    {
                                        XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + "/" + objType.Name + "[" + strFilter + "]");
                                        if (objItem != null)
                                            objItem.InnerXml = objType.InnerXml;
                                    }
                                }
                            }
                        }

                        // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
                        foreach (string strFile in Directory.GetFiles(strLoopPath, "custom*_" + strFileName))
                        {
                            objXmlFile.Load(strFile);
                            foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                            {
                                // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                                List<XmlNode> lstDelete = new List<XmlNode>();
                                foreach (XmlNode objChild in objNode.ChildNodes)
                                {
                                    XmlNode objParentNode = objChild.ParentNode;
                                    if (objParentNode != null)
                                    {
                                        string strFilter = string.Empty;
                                        if (objChild["id"] != null)
                                            strFilter = "id = \"" + objChild["id"].InnerText.Replace("&amp;", "&") + "\"";
                                        if (objChild["name"] != null)
                                        {
                                            if (!string.IsNullOrEmpty(strFilter))
                                                strFilter += " and ";
                                            strFilter += "name = \"" + objChild["name"].InnerText.Replace("&amp;", "&") + "\"";
                                        }
                                        // Only do this if the child has the name or id field since this is what we must match on.
                                        if (!string.IsNullOrEmpty(strFilter))
                                        {
                                            XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objParentNode.Name + "/" + objChild.Name + "[" + strFilter + "]");
                                            if (objItem != null)
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
                                objDocElement.AppendChild(objDoc.ImportNode(objNode, true));
                            }
                        }

                        // Load any amending data we might have, i.e. rules that only amend items instead of replacing them. Do not attempt this if we're loading the Improvements file.
                        bool blnDummy = false;
                        foreach (string strFile in Directory.GetFiles(strLoopPath, "amend*_" + strFileName))
                        {
                            objXmlFile.Load(strFile);
                            foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                            {
                                AmendNodeChildern(objDoc, objNode, "/chummer", out blnDummy);
                            }
                        }
                    }
                }

                // Load the translation file for the current base data file if the selected language is not en-us.
                if (GlobalOptions.Language != "en-us")
                {
                    // Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
                    // The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
                    if (LanguageManager.DataDoc != null)
                    {
                        foreach (XmlNode objNode in LanguageManager.DataDoc.SelectNodes("/chummer/chummer[@file = \"" + strFileName + "\"]"))
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
                                                objItem.InnerXml += "<translate>" + objChild["translate"].OuterXml + "</translate>";
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
                                            switch (strFileName)
                                            {
                                                case "skills.xml":
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
                                                    break;
                                                case "metatypes.xml":
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
                                                                                                       objMetavariant["translate"].InnerXml +
                                                                                                       "</translate>";
                                                                    }
                                                                    if (objMetavariant["page"] != null)
                                                                    {
                                                                        objMetavariantItem.InnerXml += "<altpage>" +
                                                                                                       objMetavariant["page"].InnerXml +
                                                                                                       "</altpage>";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case "martialarts.xml":
                                                    if (objChild["techniques"] != null)
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
                                                    break;
                                                case "mentors.xml":
                                                case "paragons.xml":
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
                                                    break;
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
                if (GlobalOptions.LiveCustomData)
                    objReference.XmlContent = objDoc.Clone() as XmlDocument;
                else
                    objReference.XmlContent = objDoc;
            }
            else
            {
                // A new XmlDocument is created by loading the a copy of the cached one so that we don't stuff custom content into the cached copy
                // (which we don't want and also results in multiple copies of each custom item).
                // Pull the document from cache.
                if (GlobalOptions.LiveCustomData)
                    objDoc = objReference.XmlContent.Clone() as XmlDocument;
                else
                    objDoc = objReference.XmlContent;
            }

            // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
            if (GlobalOptions.LiveCustomData && objDoc != null && strFileName != "improvements.xml")
            {
                XmlElement objDocElement = objDoc.DocumentElement;
                strPath = Path.Combine(Application.StartupPath, "livecustomdata");
                if (Directory.Exists(strPath))
                {
                    foreach (string strFile in Directory.GetFiles(strPath, "custom*_" + strFileName, SearchOption.AllDirectories))
                    {
                        objXmlFile.Load(strFile);
                        foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                        {
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>();
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                XmlNode objParentNode = objChild.ParentNode;
                                if (objParentNode != null)
                                {
                                    string strFilter = string.Empty;
                                    if (objChild["id"] != null)
                                        strFilter = "id = \"" + objChild["id"].InnerText.Replace("&amp;", "&") + "\"";
                                    if (objChild["name"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(strFilter))
                                            strFilter += " and ";
                                        strFilter += "name = \"" + objChild["name"].InnerText.Replace("&amp;", "&") + "\"";
                                    }
                                    // Only do this if the child has the name or id field since this is what we must match on.
                                    if (!string.IsNullOrEmpty(strFilter))
                                    {
                                        XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objParentNode.Name + "/" + objChild.Name + "[" + strFilter + "]");
                                        if (objItem != null)
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
                            objDocElement.AppendChild(objDoc.ImportNode(objNode, true));
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
                        .GroupBy(g => (string) g.Element("id") ?? string.Empty)
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
                        LanguageManager.GetString("Message_DuplicateGuidWarning")
                            .Replace("{0}", i.ToString())
                            .Replace("{1}", strFileName)
                            .Replace("{2}", duplicates));
                }
            }

            return objDoc;
        }

        /// <summary>
        /// Deep search a document to amend with a new node, returns whether any edits were made.
        /// If Attributes exist for the amending node, the Attributes for the original node will all be overwritten.
        /// </summary>
        /// <param name="objDoc">Document element in which to operate.</param>
        /// <param name="objAmendingNode">The amending (new) node.</param>
        /// <param name="strXPath">The current XPath in the document element that leads to where the amending node would be applied.</param>
        /// <param name="blnHasIdentifier">Whether or not the amending node or any of its children have an identifier element ("id" and/or "name" element). Can safely use a dummy boolean if this is the first call in a recursion.</param>
        private static bool AmendNodeChildern(XmlDocument objDoc, XmlNode objAmendingNode, string strXPath, out bool blnHasIdentifier)
        {
            XmlNode objAmendingNodeId = objAmendingNode["id"];
            XmlNode objAmendingNodeName = objAmendingNode["name"];
            XmlAttributeCollection objAmendingNodeAttribs = objAmendingNode.Attributes;
            blnHasIdentifier = (objAmendingNodeId != null || objAmendingNodeName != null);
            XmlNode objNodeToEdit = null;
            string strNewXPath = strXPath;

            // Fetch the old node based on identifiers present in the amending node (id and/or name)
            string strFilter = string.Empty;
            if (objAmendingNodeId != null)
                strFilter = "id = \"" + objAmendingNodeId.InnerText.Replace("&amp;", "&") + "\"";
            if (objAmendingNodeName != null)
            {
                if (!string.IsNullOrEmpty(strFilter))
                    strFilter += " and ";
                strFilter += "name = \"" + objAmendingNode["name"].InnerText.Replace("&amp;", "&") + "\"";
            }
            if (!string.IsNullOrEmpty(strFilter))
                strFilter = "[" + strFilter + "]";

            strNewXPath += "/" + objAmendingNode.Name + strFilter;

            objNodeToEdit = objDoc.SelectSingleNode(strNewXPath);
            // We don't want to edit a random element if we don't have an identifier, so select the one whose text matches our own if it exists.
            if (!blnHasIdentifier && objAmendingNodeAttribs?["requireinnertextmatch"]?.InnerText == "yes")
            {
                blnHasIdentifier = true;
                objNodeToEdit = null;
                foreach (XmlNode objLoopNode in objDoc.SelectNodes(strNewXPath))
                {
                    if (objLoopNode.Name == objAmendingNode.Name && objLoopNode.InnerText == objAmendingNode.InnerText)
                    {
                        objNodeToEdit = objLoopNode;
                        break;
                    }
                }
            }
            bool blnHasElementChildren = false;
            if (objAmendingNode.HasChildNodes)
            {
                foreach (XmlNode objChild in objAmendingNode.ChildNodes)
                {
                    if (objChild.NodeType == XmlNodeType.Element)
                    {
                        blnHasElementChildren = true;
                        break;
                    }
                }
            }
            if (objNodeToEdit != null && (blnHasIdentifier || blnHasElementChildren || objAmendingNodeAttribs?["remove"]?.InnerText == "yes"))
            {
                // If the old node exists and the amending node has the attribute 'remove="yes"', then the old node is completely erased.
                if (objAmendingNodeAttribs?["remove"]?.InnerText == "yes")
                {
                    objDoc.SelectSingleNode(strXPath).RemoveChild(objNodeToEdit);
                }
                else
                {
                    XmlAttributeCollection objNodeToEditAttribs = objNodeToEdit.Attributes;
                    // Attributes are the only thing that is overwritten completely
                    if (objNodeToEditAttribs != null && objAmendingNodeAttribs != null && objAmendingNodeAttribs.Count > 0)
                    {
                        objNodeToEditAttribs.RemoveAll();
                        foreach (XmlAttribute objNewAttribute in objAmendingNodeAttribs)
                        {
                            if (objNewAttribute.Name != "requireinnertextmatch" && objNewAttribute.Name != "remove" && objNewAttribute.Name != "addifnotfound")
                                objNodeToEditAttribs.Append(objNewAttribute);
                        }
                    }
                    // If the amending node has children elements, run this method on all of its children.
                    if (blnHasElementChildren)
                    {
                        foreach (XmlNode objChild in objAmendingNode.ChildNodes)
                        {
                            if (objChild.NodeType != XmlNodeType.Element)
                                continue;
                            bool blnLoopHasIdentifier = false;
                            // For each child, if no edits were made, but the child has an identifier (id and/or name field), then append the child to the old node.
                            if (!AmendNodeChildern(objDoc, objChild, strNewXPath, out blnLoopHasIdentifier))
                            {
                                if (blnLoopHasIdentifier)
                                {
                                    objNodeToEdit.AppendChild(objDoc.ImportNode(objChild, true));
                                }
                            }
                            blnHasIdentifier = blnHasIdentifier || blnLoopHasIdentifier;
                        }
                    }
                    // If neither the amending node nor the old node has children elements, overwrite the old node with the amending one.
                    else
                    {
                        foreach (XmlNode objChild in objNodeToEdit.ChildNodes)
                        {
                            if (objChild.NodeType == XmlNodeType.Element)
                            {
                                blnHasElementChildren = true;
                                break;
                            }
                        }
                        if (!blnHasElementChildren)
                            objNodeToEdit.InnerXml = objAmendingNode.InnerXml;
                    }
                }
                return true;
            }
            // If there aren't any old nodes found and the amending node is tagged as needing to be added should this be the case, then append the entire amending node to the XPath.
            else if (objAmendingNodeAttribs?["addifnotfound"]?.InnerText != "no")
            {
                return objDoc.SelectSingleNode(strXPath)?.AppendChild(objDoc.ImportNode(objAmendingNode, true)) != null;
            }

            return false;
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        public static void Verify(string strLanguage, List<string> lstBooks)
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
                                                if (objChild["techniques"] != null)
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
