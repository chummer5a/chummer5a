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
using System.Xml;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Chummer
{
    // ReSharper disable InconsistentNaming
    public static class XmlManager
    {
        /// <summary>
        /// Used to cache XML files so that they do not need to be loaded and translated each time an object wants the file.
        /// </summary>
        private sealed class XmlReference
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
            /// Language of the XML file.
            /// </summary>
            public string Language { get; set; } = GlobalOptions.DefaultLanguage;

            /// <summary>
            /// Whether or not the XML file has been successfully checked for duplicate guids.
            /// </summary>
            public bool DuplicatesChecked { get; set; }

            /// <summary>
            /// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public XmlDocument XmlContent { get; set; } = new XmlDocument();
        }

        private static readonly HashSet<XmlReference> s_LstXmlDocuments = new HashSet<XmlReference>();
        private static readonly object s_LstXmlDocumentsLock = new object();
        private static readonly List<string> s_LstDataDirectories = new List<string>();

        #region Constructor
        static XmlManager()
        {
            s_LstDataDirectories.Add(Path.Combine(Application.StartupPath, "data"));
            foreach (CustomDataDirectoryInfo objCustomDataDirectory in GlobalOptions.CustomDataDirectoryInfo.Where(x => x.Enabled))
            {
                s_LstDataDirectories.Add(objCustomDataDirectory.Path);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load the selected XML file and its associated custom file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        public static XmlDocument Load(string strFileName, string strLanguage = "", bool blnLoadFile = false)
        {
            bool blnFileFound = false;
            string strPath = string.Empty;
            foreach (string strDirectory in s_LstDataDirectories)
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
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;

            // Look to see if this XmlDocument is already loaded.
            XmlReference objReference = null;
            lock (s_LstXmlDocumentsLock)
            {
                objReference = s_LstXmlDocuments.FirstOrDefault(x => x.FileName == strFileName);
                if (objReference == null || blnLoadFile)
                {
                    // The file was not found in the reference list, so it must be loaded.
                    objReference = new XmlReference();
                    blnLoadFile = true;
                    s_LstXmlDocuments.Add(objReference);
                }
                // The file was found in the List, so check the last write time and language.
                else if (datDate != objReference.FileDate || strLanguage != objReference.Language)
                {
                    // The last write time and/or language does not match, so it must be reloaded.
                    blnLoadFile = true;
                }
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
                    foreach (string strLoopPath in s_LstDataDirectories)
                    {
                        foreach (string strFile in Directory.GetFiles(strLoopPath, "override*_" + strFileName))
                        {
                            objXmlFile.Load(strFile);
                            foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                            {
                                foreach (XmlNode objType in objNode.ChildNodes)
                                {
                                    string strFilter = string.Empty;
                                    XmlNode xmlIdNode = objType["id"];
                                    if (xmlIdNode != null)
                                        strFilter = "id = \"" + xmlIdNode.InnerText.Replace("&amp;", "&") + '\"';
                                    else
                                    {
                                        xmlIdNode = objType["name"];
                                        if (xmlIdNode != null)
                                            strFilter = "name = \"" + xmlIdNode.InnerText.Replace("&amp;", "&") + '\"';
                                    }
                                    // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                                    XmlNodeList objAmendingNodeExtraIds = objType.SelectNodes("child::*[@isidnode = \"True\"]");
                                    foreach (XmlNode objExtraId in objAmendingNodeExtraIds)
                                    {
                                        if (!string.IsNullOrEmpty(strFilter))
                                            strFilter += " and ";
                                        strFilter += objExtraId.Name + " = \"" + objExtraId.InnerText.Replace("&amp;", "&") + '\"';
                                    }
                                    if (!string.IsNullOrEmpty(strFilter))
                                    {
                                        XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + '/' + objType.Name + '[' + strFilter + ']');
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
                                        XmlNode xmlIdNode = objChild["id"];
                                        if (xmlIdNode != null)
                                            strFilter = "id = \"" + xmlIdNode.InnerText.Replace("&amp;", "&") + '\"';
                                        XmlNode xmlNameNode = objChild["name"];
                                        if (xmlNameNode != null)
                                        {
                                            if (!string.IsNullOrEmpty(strFilter))
                                                strFilter += " and ";
                                            strFilter += "name = \"" + xmlNameNode.InnerText.Replace("&amp;", "&") + '\"';
                                        }
                                        // Only do this if the child has the name or id field since this is what we must match on.
                                        if (!string.IsNullOrEmpty(strFilter))
                                        {
                                            XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objParentNode.Name + '/' + objChild.Name + '[' + strFilter + ']');
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
                        foreach (string strFile in Directory.GetFiles(strLoopPath, "amend*_" + strFileName))
                        {
                            objXmlFile.Load(strFile);
                            foreach (XmlNode objNode in objXmlFile.SelectNodes("/chummer/*"))
                            {
                                AmendNodeChildern(objDoc, objNode, "/chummer");
                            }
                        }
                    }
                }

                // Load the translation file for the current base data file if the selected language is not en-us.
                if (strLanguage != GlobalOptions.DefaultLanguage)
                {
                    // Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
                    // The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
                    XmlDocument objDataDoc = LanguageManager.GetDataDocument(strLanguage);
                    if (objDataDoc != null)
                    {
                        XmlNode xmlBaseChummerNode = objDoc.SelectSingleNode("/chummer");
                        foreach (XmlNode objType in objDataDoc.SelectNodes("/chummer/chummer[@file = \"" + strFileName + "\"]/*"))
                        {
                            AppendTranslations(objDoc, objType, xmlBaseChummerNode);
                        }
                    }
                }

                // Cache the merged document and its relevant information.
                objReference.FileDate = datDate;
                objReference.FileName = strFileName;
                objReference.Language = strLanguage;
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
            bool blnHasLiveCustomData = false;
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
                            blnHasLiveCustomData = true;
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>();
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                XmlNode objParentNode = objChild.ParentNode;
                                if (objParentNode != null)
                                {
                                    string strFilter = string.Empty;
                                    if (objChild["id"] != null)
                                        strFilter = "id = \"" + objChild["id"].InnerText.Replace("&amp;", "&") + '\"';
                                    if (objChild["name"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(strFilter))
                                            strFilter += " and ";
                                        strFilter += "name = \"" + objChild["name"].InnerText.Replace("&amp;", "&") + '\"';
                                    }
                                    // Only do this if the child has the name or id field since this is what we must match on.
                                    if (!string.IsNullOrEmpty(strFilter))
                                    {
                                        XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objParentNode.Name + '/' + objChild.Name + '[' + strFilter + ']');
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

            // Check for non-unique guids and non-guid formatted ids in the loaded XML file. Ignore improvements.xml since the ids are used in a different way.
            if (strFileName == "improvements.xml" || (objReference.DuplicatesChecked && !blnHasLiveCustomData)) return objDoc;
            {
                foreach (XmlNode objNode in objDoc.SelectNodes("/chummer/*"))
                {
                    // Only process nodes that have children and are not the version node
                    if (objNode.Name != "version" && objNode.HasChildNodes)
                    {
                        // Parse the node into an XDocument for LINQ parsing.
                        CheckIdNodes(XDocument.Parse(objNode.OuterXml), strFileName);
                    }
                }

                objReference.DuplicatesChecked = true;
            }

            return objDoc;
        }

        public static XPathNavigator GetFastNavigator(this XmlDocument xmlDoc)
        {
            MemoryStream memStream = new MemoryStream();
            xmlDoc.Save(memStream);
            memStream.Position = 0;
            return new XPathDocument(memStream).CreateNavigator();
        }

        private static void CheckIdNodes(XContainer xmlParentNode, string strFileName)
        {
            HashSet<string> lstDuplicateIDs = new HashSet<string>();
            List<string> lstItemsWithMalformedIDs = new List<string>();
            // Not a dictionary specifically so duplicates can be caught. Item1 is ID, Item2 is the item's name.
            List<Tuple<string, string>> lstItemsWithIDs = new List<Tuple<string, string>>();

            foreach (XElement objLoopNode in xmlParentNode.Elements())
            {
                string strId = (string)objLoopNode.Element("id") ?? string.Empty;
                if (!string.IsNullOrEmpty(strId))
                {
                    string strItemName = (string)objLoopNode.Element("name") ?? (string)objLoopNode.Element("stage") ?? (string)objLoopNode.Element("category") ?? strId;
                    if (!lstDuplicateIDs.Contains(strId) && lstItemsWithIDs.Any(x => x.Item1 == strId))
                    {
                        lstDuplicateIDs.Add(strId);
                        if (strItemName == strId)
                            strItemName = string.Empty;
                    }
                    if (!strId.IsGuid())
                        lstItemsWithMalformedIDs.Add(strItemName);
                    lstItemsWithIDs.Add(new Tuple<string, string>(strId, strItemName));
                }

                // Perform recursion so that nested elements that also have ids are also checked (e.g. Metavariants)
                CheckIdNodes(objLoopNode, strFileName);
            }

            if (lstDuplicateIDs.Count > 0)
            {
                string strDuplicatesNames = string.Join("\n", lstItemsWithIDs.Where(x => lstDuplicateIDs.Contains(x.Item1) && !string.IsNullOrEmpty(x.Item2)).Select(x => x.Item2));
                MessageBox.Show(
                    LanguageManager.GetString("Message_DuplicateGuidWarning", GlobalOptions.Language)
                        .Replace("{0}", lstDuplicateIDs.Count.ToString())
                        .Replace("{1}", strFileName)
                        .Replace("{2}", strDuplicatesNames));
            }

            if (lstItemsWithMalformedIDs.Count > 0)
            {
                string strMalformedIdNames = string.Join("\n", lstItemsWithMalformedIDs);
                MessageBox.Show(
                    LanguageManager.GetString("Message_NonGuidIdWarning", GlobalOptions.Language)
                        .Replace("{0}", lstItemsWithMalformedIDs.Count.ToString())
                        .Replace("{1}", strFileName)
                        .Replace("{2}", strMalformedIdNames));
            }
        }

        private static void AppendTranslations(XmlDocument xmlDataDocument, XmlNode xmlTranslationListParentNode, XmlNode xmlDataParentNode)
        {
            foreach (XmlNode objChild in xmlTranslationListParentNode.ChildNodes)
            {
                XmlNode xmlItem = null;
                string strChildName = objChild["id"]?.InnerText;
                if (!string.IsNullOrEmpty(strChildName))
                {
                    xmlItem = xmlDataParentNode.SelectSingleNode(xmlTranslationListParentNode.Name + '/' + objChild.Name + "[id = \"" + strChildName + "\"]");
                }
                if (xmlItem == null)
                {
                    strChildName = objChild["name"]?.InnerText.Replace("&amp;", "&");
                    if (!string.IsNullOrEmpty(strChildName))
                    {
                        xmlItem = xmlDataParentNode.SelectSingleNode(xmlTranslationListParentNode.Name + '/' + objChild.Name + "[name = \"" + strChildName + "\"]");
                    }
                }
                // If this is a translatable item, find the proper node and add/update this information.
                if (xmlItem != null)
                {
                    XmlNode xmlLoopNode = objChild["translate"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altpage"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altcode"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altnotes"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altadvantage"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altdisadvantage"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    xmlLoopNode = objChild["altnameonpage"];
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlDataDocument.ImportNode(xmlLoopNode, true));

                    string strTranslate = objChild.Attributes?["translate"]?.InnerXml;
                    if (!string.IsNullOrEmpty(strTranslate))
                    {
                        // Handle Category name translations.
                        (xmlItem as XmlElement)?.SetAttribute("translate", strTranslate);
                    }

                    // Sub-children to also process with the translation
                    XmlNode xmlSubItemsNode = objChild["specs"];
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild["metavariants"];
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild["choices"];
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild["talents"];
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild["versions"];
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                }
                else
                {
                    string strTranslate = objChild.Attributes?["translate"]?.InnerXml;
                    if (!string.IsNullOrEmpty(strTranslate))
                    {
                        // Handle Category name translations.
                        XmlElement objItem = xmlDataParentNode.SelectSingleNode(xmlTranslationListParentNode.Name + '/' + objChild.Name + "[. = \"" + objChild.InnerXml.Replace("&amp;", "&") + "\"]") as XmlElement;
                        // Expected result is null if not found.
                        objItem?.SetAttribute("translate", strTranslate);
                    }
                }
            }
        }

        /// <summary>
        /// Deep search a document to amend with a new node.
        /// If Attributes exist for the amending node, the Attributes for the original node will all be overwritten.
        /// </summary>
        /// <param name="objDoc">Document element in which to operate.</param>
        /// <param name="objAmendingNode">The amending (new) node.</param>
        /// <param name="strXPath">The current XPath in the document element that leads to the target node(s) where the amending node would be applied.</param>
        private static void AmendNodeChildern(XmlDocument objDoc, XmlNode objAmendingNode, string strXPath)
        {
            // Fetch the old node based on identifiers present in the amending node (id or name)
            string strFilter = string.Empty;
            XmlNode objAmendingNodeId = objAmendingNode["id"];
            if (objAmendingNodeId != null)
            {
                strFilter = "id = \"" + objAmendingNodeId.InnerText.Replace("&amp;", "&") + '\"';
            }
            else
            {
                objAmendingNodeId = objAmendingNode["name"];
                if (objAmendingNodeId != null)
                {
                    strFilter = "name = \"" + objAmendingNodeId.InnerText.Replace("&amp;", "&") + '\"';
                }
            }
            // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
            foreach (XmlNode objExtraId in objAmendingNode.SelectNodes("child::*[@isidnode = \"True\"]"))
            {
                if (!string.IsNullOrEmpty(strFilter))
                    strFilter += " and ";
                strFilter += objExtraId.Name + " = \"" + objExtraId.InnerText.Replace("&amp;", "&") + '\"';
            }

            string strOperation = string.Empty;
            bool blnAddIfNotFound = true;
            XmlAttributeCollection objAmendingNodeAttribs = objAmendingNode.Attributes;
            if (objAmendingNodeAttribs != null)
            {
                // This attribute is not used by the node itself, so it can be removed to speed up node importing later on.
                objAmendingNodeAttribs.RemoveNamedItem("isidnode");

                // Gets the custom XPath filter defined for what children to fetch, so add that into the XPath filter for targeting nodes if it exists.
                XmlNode objCustomXPath = objAmendingNodeAttribs.RemoveNamedItem("xpathfilter");
                if (objCustomXPath != null)
                {
                    if (!string.IsNullOrEmpty(strFilter))
                        strFilter += " and ";
                    strFilter += '(' + objCustomXPath.InnerText.Replace("&amp;", "&") + ')';
                }

                // Gets the specific operation to execute on this node.
                XmlNode objAmendOperation = objAmendingNodeAttribs.RemoveNamedItem("amendoperation");
                if (objAmendOperation != null)
                {
                    strOperation = objAmendOperation.InnerText;
                }

                // Get info on whether this node should be appended if no target node is found
                XmlNode objAddIfNotFound = objAmendingNodeAttribs.RemoveNamedItem("addifnotfound");
                if (objAddIfNotFound != null)
                {
                    blnAddIfNotFound = objAddIfNotFound.InnerText == bool.TrueString;
                }
            }

            if (!string.IsNullOrEmpty(strFilter))
                strFilter = '[' + strFilter + ']';

            string strNewXPath = strXPath + '/' + objAmendingNode.Name + strFilter;

            XmlNodeList objNodesToEdit = objDoc.SelectNodes(strNewXPath);

            List<XmlNode> lstElementChildren = null;
            // Pre-cache list of elements if we don't have an operation specified or have recurse specified
            if ((string.IsNullOrEmpty(strOperation) || strOperation == "recurse"))
            {
                lstElementChildren = new List<XmlNode>();
                if (objAmendingNode.HasChildNodes)
                {
                    foreach (XmlNode objChild in objAmendingNode.ChildNodes)
                    {
                        if (objChild.NodeType == XmlNodeType.Element)
                        {
                            lstElementChildren.Add(objChild);
                        }
                    }
                }
            }

            switch (strOperation)
            {
                // These operations are supported
                case "remove":
                case "replace":
                case "append":
                    break;
                case "recurse":
                    // Operation only supported if we have children
                    if (lstElementChildren.Count > 0)
                        break;
                    goto default;
                // If no supported operation is specified, the default is...
                default:
                    // ..."recurse" if we have children...
                    if (lstElementChildren.Count > 0)
                        strOperation = "recurse";
                    // ..."append" if we don't have children and there's no target...
                    else if (objNodesToEdit.Count == 0)
                        strOperation = "append";
                    // ..."replace" if we don't have children and there are one or more targets.
                    else
                        strOperation = "replace";
                    break;
            }

            // We found nodes to target with the amend!
            if (objNodesToEdit.Count > 0)
            {
                // Recurse is special in that it doesn't directly target nodes, but does so indirectly through strNewXPath...
                if (strOperation == "recurse")
                {
                    foreach (XmlNode objChild in lstElementChildren)
                    {
                        AmendNodeChildern(objDoc, objChild, strNewXPath);
                    }
                }
                // ... otherwise loop through any nodes that satisfy the XPath filter.
                else
                {
                    foreach (XmlNode objNodeToEdit in objNodesToEdit)
                    {
                        XmlNode objParentNode = objNodeToEdit.ParentNode;
                        // If the old node exists and the amending node has the attribute 'amendoperation="remove"', then the old node is completely erased.
                        if (strOperation == "remove")
                        {
                            objParentNode.RemoveChild(objNodeToEdit);
                        }
                        else
                        {
                            switch (strOperation)
                            {
                                case "append":
                                    if (objAmendingNode.HasChildNodes)
                                    {
                                        foreach (XmlNode objChild in objAmendingNode.ChildNodes)
                                        {
                                            XmlNodeType eChildNodeType = objChild.NodeType;

                                            // Skip adding comments, they're pointless for the purposes of Chummer5a's code
                                            if (eChildNodeType == XmlNodeType.Comment)
                                                continue;

                                            // Text, Attributes, and CDATA should add their values to existing children of the same type if possible
                                            if (eChildNodeType == XmlNodeType.Text ||
                                                eChildNodeType == XmlNodeType.Attribute ||
                                                eChildNodeType == XmlNodeType.CDATA)
                                            {
                                                bool blnItemFound = false;
                                                if (objNodeToEdit.HasChildNodes)
                                                {
                                                    foreach (XmlNode objChildToEdit in objNodeToEdit.ChildNodes)
                                                    {
                                                        if (objChildToEdit.NodeType == eChildNodeType)
                                                        {
                                                            if (eChildNodeType != XmlNodeType.Attribute || objChildToEdit.Name == objChild.Name)
                                                            {
                                                                objChildToEdit.Value += objChild.Value;
                                                                blnItemFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (blnItemFound)
                                                    continue;
                                            }

                                            objNodeToEdit.AppendChild(objDoc.ImportNode(objChild, true));
                                        }
                                    }
                                    break;
                                case "replace":
                                    objParentNode.ReplaceChild(objDoc.ImportNode(objAmendingNode, true), objNodeToEdit);
                                    break;
                            }
                        }
                    }
                }
            }
            // If there aren't any old nodes found and the amending node is tagged as needing to be added should this be the case, then append the entire amending node to the XPath.
            else if (strOperation == "append" || (strOperation == "recurse" || strOperation == "replace") && blnAddIfNotFound)
            {
                foreach (XmlNode objParentNode in objDoc.SelectNodes(strXPath))
                {
                    objParentNode.AppendChild(objDoc.ImportNode(objAmendingNode, true));
                }
            }
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        public static void Verify(string strLanguage, List<string> lstBooks)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return;
            XmlDocument objLanguageDoc = new XmlDocument();
            string languageDirectoryPath = Path.Combine(Application.StartupPath, "lang");
            string strFilePath = Path.Combine(languageDirectoryPath, strLanguage + "_data.xml");
            objLanguageDoc.Load(strFilePath);

            XPathNavigator objLanguageNavigator = objLanguageDoc.GetFastNavigator();

            string strLangPath = Path.Combine(languageDirectoryPath, "results_" + strLanguage + ".xml");
            FileStream objStream = new FileStream(strLangPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };

            objWriter.WriteStartDocument();
            // <results>
            objWriter.WriteStartElement("results");

            string strPath = Path.Combine(Application.StartupPath, "data");
            foreach (string strFile in Directory.GetFiles(strPath, "*.xml"))
            {
                string strFileName = Path.GetFileName(strFile);

                if (!string.IsNullOrEmpty(strFileName) &&
                    // Do not bother to check custom files.
                    !strFileName.StartsWith("amend") &&
                    !strFileName.StartsWith("custom") &&
                    !strFileName.StartsWith("override") &&
                    // These file types don't have translations and/or don't properly support them
                    !strFile.EndsWith("packs.xml") &&
                    !strFile.EndsWith("lifemodules.xml") &&
                    !strFile.EndsWith("sheets.xml"))
                {
                    // Load the current English file.
                    XPathNavigator objEnglishDoc = Load(strFileName).GetFastNavigator();
                    XPathNavigator objEnglishRoot = objEnglishDoc.SelectSingleNode("/chummer");

                    // First pass: make sure the document exists.
                    bool blnExists = false;
                    XPathNavigator objLanguageRoot = objLanguageNavigator.SelectSingleNode("/chummer/chummer[@file = \"" + strFileName + "\"]");
                    if (objLanguageRoot != null)
                        blnExists = true;

                    // <file name="x" needstobeadded="y">
                    objWriter.WriteStartElement("file");
                    objWriter.WriteAttributeString("name", strFileName);

                    if (blnExists)
                    {
                        foreach (XPathNavigator objType in objEnglishRoot.SelectChildren(XPathNodeType.Element))
                        {
                            string strTypeName = objType.Name;
                            bool blnTypeWritten = false;
                            foreach (XPathNavigator objChild in objType.SelectChildren(XPathNodeType.Element))
                            {
                                // If the Node has a source element, check it and see if it's in the list of books that were specified.
                                // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                                bool blnContinue = true;
                                XPathNavigator xmlSource = objChild.SelectSingleNode("source");
                                if (xmlSource != null)
                                {
                                    blnContinue = false;
                                    foreach (string strBook in lstBooks)
                                    {
                                        if (strBook == xmlSource.Value)
                                        {
                                            blnContinue = true;
                                            break;
                                        }
                                    }
                                }

                                if (blnContinue)
                                {
                                    if (strTypeName != "version" && !((strTypeName == "costs" || strTypeName == "safehousecosts" || strTypeName == "comforts" || strTypeName == "neighborhoods" || strTypeName == "securities") && strFile.EndsWith("lifestyles.xml")))
                                    {
                                        string strChildName = objChild.Name;
                                        XPathNavigator xmlTranslatedType = objLanguageRoot.SelectSingleNode(strTypeName);
                                        XPathNavigator xmlName = objChild.SelectSingleNode("name");
                                        // Look for a matching entry in the Language file.
                                        if (xmlName != null)
                                        {
                                            string strChildNameElement = xmlName.Value;
                                            XPathNavigator xmlNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[name = \"" + strChildNameElement + "\"]");
                                            if (xmlNode != null)
                                            {
                                                // A match was found, so see what elements, if any, are missing.
                                                bool blnTranslate = false;
                                                bool blnAltPage = false;
                                                bool blnAdvantage = false;
                                                bool blnDisadvantage = false;

                                                if (objChild.HasChildren)
                                                {
                                                    if (xmlNode.SelectSingleNode("translate") != null)
                                                        blnTranslate = true;

                                                    // Do not mark page as missing if the original does not have it.
                                                    if (objChild.SelectSingleNode("page") != null)
                                                    {
                                                        if (xmlNode.SelectSingleNode("altpage") != null)
                                                            blnAltPage = true;
                                                    }
                                                    else
                                                        blnAltPage = true;

                                                    if (strFile.EndsWith("mentors.xml") || strFile.EndsWith("paragons.xml"))
                                                    {
                                                        if (xmlNode.SelectSingleNode("altadvantage") != null)
                                                            blnAdvantage = true;
                                                        if (xmlNode.SelectSingleNode("altdisadvantage") != null)
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
                                                    if (xmlNode.SelectSingleNode("@translate") != null)
                                                        blnTranslate = true;
                                                }

                                                // At least one pice of data was missing so write out the result node.
                                                if (!blnTranslate || !blnAltPage || !blnAdvantage || !blnDisadvantage)
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        objWriter.WriteStartElement(strTypeName);
                                                    }
                                                    // <results>
                                                    objWriter.WriteStartElement(strChildName);
                                                    objWriter.WriteElementString("name", strChildNameElement);
                                                    if (!blnTranslate)
                                                        objWriter.WriteElementString("missing", "translate");
                                                    if (!blnAltPage)
                                                        objWriter.WriteElementString("missing", "altpage");
                                                    if (!blnAdvantage)
                                                        objWriter.WriteElementString("missing", "altadvantage");
                                                    if (!blnDisadvantage)
                                                        objWriter.WriteElementString("missing", "altdisadvantage");
                                                    // </results>
                                                    objWriter.WriteEndElement();
                                                }
                                            }
                                            else
                                            {
                                                if (!blnTypeWritten)
                                                {
                                                    blnTypeWritten = true;
                                                    objWriter.WriteStartElement(strTypeName);
                                                }
                                                // No match was found, so write out that the data item is missing.
                                                // <result>
                                                objWriter.WriteStartElement(strChildName);
                                                objWriter.WriteAttributeString("needstobeadded", bool.TrueString);
                                                objWriter.WriteElementString("name", strChildNameElement);
                                                // </result>
                                                objWriter.WriteEndElement();
                                            }

                                            if (strFileName == "metatypes.xml")
                                            {
                                                XPathNavigator xmlMetavariants = objChild.SelectSingleNode("metavariants");
                                                if (xmlMetavariants != null)
                                                {
                                                    foreach (XPathNavigator objMetavariant in xmlMetavariants.Select("metavariant"))
                                                    {
                                                        string strMetavariantName = objMetavariant.SelectSingleNode("name").Value;
                                                        XPathNavigator objTranslate = objLanguageRoot.SelectSingleNode("metatypes/metatype[name = \"" + strChildNameElement + "\"]/metavariants/metavariant[name = \"" + strMetavariantName + "\"]");
                                                        if (objTranslate != null)
                                                        {
                                                            bool blnTranslate = objTranslate.SelectSingleNode("translate") != null;
                                                            bool blnAltPage = objTranslate.SelectSingleNode("altpage") != null;
                                                            
                                                            // Item exists, so make sure it has its translate attribute populated.
                                                            if (!blnTranslate || !blnAltPage)
                                                            {
                                                                if (!blnTypeWritten)
                                                                {
                                                                    blnTypeWritten = true;
                                                                    objWriter.WriteStartElement(strTypeName);
                                                                }
                                                                // <result>
                                                                objWriter.WriteStartElement("metavariants");
                                                                objWriter.WriteStartElement("metavariant");
                                                                objWriter.WriteElementString("name", strMetavariantName);
                                                                if (!blnTranslate)
                                                                    objWriter.WriteElementString("missing", "translate");
                                                                if (!blnAltPage)
                                                                    objWriter.WriteElementString("missing", "altpage");
                                                                objWriter.WriteEndElement();
                                                                // </result>
                                                                objWriter.WriteEndElement();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!blnTypeWritten)
                                                            {
                                                                blnTypeWritten = true;
                                                                objWriter.WriteStartElement(strTypeName);
                                                            }
                                                            // <result>
                                                            objWriter.WriteStartElement("metavariants");
                                                            objWriter.WriteStartElement("metavariant");
                                                            objWriter.WriteAttributeString("needstobeadded", bool.TrueString);
                                                            objWriter.WriteElementString("name", strMetavariantName);
                                                            objWriter.WriteEndElement();
                                                            // </result>
                                                            objWriter.WriteEndElement();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (strChildName == "#comment")
                                        {
                                            //Ignore this node, as it's a comment node.
                                        }
                                        else
                                        {
                                            string strChildInnerText = objChild.Value;
                                            if (!string.IsNullOrEmpty(strChildInnerText))
                                            {
                                                // The item does not have a name which means it should have a translate CharacterAttribute instead.
                                                XPathNavigator objNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[text() = \"" + strChildInnerText + "\"]");
                                                if (objNode != null)
                                                {
                                                    // Make sure the translate attribute is populated.
                                                    if (objNode.SelectSingleNode("@translate") == null)
                                                    {
                                                        if (!blnTypeWritten)
                                                        {
                                                            blnTypeWritten = true;
                                                            objWriter.WriteStartElement(strTypeName);
                                                        }
                                                        // <result>
                                                        objWriter.WriteStartElement(strChildName);
                                                        objWriter.WriteElementString("name", strChildInnerText);
                                                        objWriter.WriteElementString("missing", "translate");
                                                        // </result>
                                                        objWriter.WriteEndElement();
                                                    }
                                                }
                                                else
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        objWriter.WriteStartElement(strTypeName);
                                                    }
                                                    // No match was found, so write out that the data item is missing.
                                                    // <result>
                                                    objWriter.WriteStartElement(strChildName);
                                                    objWriter.WriteAttributeString("needstobeadded", bool.TrueString);
                                                    objWriter.WriteElementString("name", strChildInnerText);
                                                    // </result>
                                                    objWriter.WriteEndElement();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (blnTypeWritten)
                                objWriter.WriteEndElement();
                        }

                        // Now loop through the translation file and determine if there are any entries in there that are not part of the base content.
                        foreach (XPathNavigator objType in objLanguageRoot.SelectChildren(XPathNodeType.Element))
                        {
                            foreach (XPathNavigator objChild in objType.SelectChildren(XPathNodeType.Element))
                            {
                                string strChildNameElement = objChild.SelectSingleNode("name")?.Value;
                                // Look for a matching entry in the English file.
                                if (!string.IsNullOrEmpty(strChildNameElement))
                                {
                                    string strChildName = objChild.Name;
                                    XPathNavigator objNode = objEnglishRoot.SelectSingleNode("/chummer/" + objType.Name + '/' + strChildName + "[name = \"" + strChildNameElement + "\"]");
                                    if (objNode == null)
                                    {
                                        // <noentry>
                                        objWriter.WriteStartElement("noentry");
                                        objWriter.WriteStartElement(strChildName);
                                        objWriter.WriteElementString("name", strChildNameElement);
                                        objWriter.WriteEndElement();
                                        // </noentry>
                                        objWriter.WriteEndElement();
                                    }
                                }
                            }
                        }
                    }
                    else
                        objWriter.WriteAttributeString("needstobeadded", bool.TrueString);

                    // </file>
                    objWriter.WriteEndElement();
                }
            }

            // </results>
            objWriter.WriteEndElement();
            objWriter.WriteEndDocument();
            objWriter.Close();
        }
        #endregion
    }
}
