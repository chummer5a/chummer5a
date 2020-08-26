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
using System.Text.RegularExpressions;
using System.Xml;
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
            public XmlDocument XmlContent { get; set; } = new XmlDocument { XmlResolver = null };
        }

        private static readonly HashSet<XmlReference> s_LstXmlDocuments = new HashSet<XmlReference>();
        private static readonly object s_LstXmlDocumentsLock = new object();
        private static readonly List<string> s_LstDataDirectories = new List<string>(30);
        private static readonly object s_SetFilesWithCachedDocsLock = new object();
        private static readonly HashSet<string> s_SetFilesWithCachedDocs = new HashSet<string>();

        #region Constructor
        static XmlManager()
        {
            s_LstDataDirectories.Add(Path.Combine(Utils.GetStartupPath, "data"));
            foreach (CustomDataDirectoryInfo objCustomDataDirectory in GlobalOptions.CustomDataDirectoryInfo)
            {
                if (objCustomDataDirectory.Enabled)
                    s_LstDataDirectories.Add(objCustomDataDirectory.Path);
            }
        }
        #endregion

        #region Methods
        public static void RebuildDataDirectoryInfo()
        {
            lock (s_SetFilesWithCachedDocsLock)
                s_SetFilesWithCachedDocs.Clear();
            s_LstDataDirectories.Clear();
            s_LstDataDirectories.Add(Path.Combine(Utils.GetStartupPath, "data"));
            foreach (CustomDataDirectoryInfo objCustomDataDirectory in GlobalOptions.CustomDataDirectoryInfo)
            {
                if (objCustomDataDirectory.Enabled)
                    s_LstDataDirectories.Add(objCustomDataDirectory.Path);
            }
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [Annotations.NotNull]
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
                return new XmlDocument { XmlResolver = null };
            }

            lock (s_SetFilesWithCachedDocsLock)
                if (!s_SetFilesWithCachedDocs.Contains(strFileName))
                    blnLoadFile = true;

            DateTime datDate = File.GetLastWriteTime(strPath);
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;

            // Look to see if this XmlDocument is already loaded.
            XmlReference objReference;
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
            XmlDocument objXmlFile = new XmlDocument { XmlResolver = null };

            if (blnLoadFile)
            {
                objDoc = new XmlDocument {XmlResolver = null};
                // write the root chummer node.
                XmlNode objCont = objDoc.CreateElement("chummer");
                objDoc.AppendChild(objCont);
                XmlElement objDocElement = objDoc.DocumentElement;
                // Load the base file and retrieve all of the child nodes.
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strPath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            objXmlFile.Load(objXmlReader);

                    if (objDocElement != null)
                    {
                        using (XmlNodeList xmlNodeList = objXmlFile.SelectNodes("/chummer/*"))
                        {
                            if (xmlNodeList?.Count > 0)
                            {
                                foreach (XmlNode objNode in xmlNodeList)
                                {
                                    // Append the entire child node to the new document.
                                    objDocElement.AppendChild(objDoc.ImportNode(objNode, true));
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                }
                catch (XmlException)
                {
                }

                // Load any override data files the user might have. Do not attempt this if we're loading the Improvements file.
                if (strFileName != "improvements.xml")
                {
                    foreach (string strLoopPath in s_LstDataDirectories)
                    {
                        DoProcessCustomDataFiles(objXmlFile, objDoc, strLoopPath, strFileName);
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
                        using (XmlNodeList xmlTranslationTypeNodeList = objDataDoc.SelectNodes("/chummer/chummer[@file = " + strFileName.CleanXPath() + "]/*"))
                        {
                            if (xmlTranslationTypeNodeList?.Count > 0)
                            {
                                foreach (XmlNode objType in xmlTranslationTypeNodeList)
                                {
                                    AppendTranslations(objDoc, objType, xmlBaseChummerNode);
                                }
                            }
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

                lock (s_SetFilesWithCachedDocsLock)
                    s_SetFilesWithCachedDocs.Add(strFileName);
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

            objDoc = objDoc ?? new XmlDocument {XmlResolver = null};
            if (strFileName == "improvements.xml")
                return objDoc;

            // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
            bool blnHasLiveCustomData = false;
            if (GlobalOptions.LiveCustomData)
            {
                strPath = Path.Combine(Utils.GetStartupPath, "livecustomdata");
                if (Directory.Exists(strPath))
                {
                    blnHasLiveCustomData = DoProcessCustomDataFiles(objXmlFile, objDoc, strPath, strFileName, SearchOption.AllDirectories);
                }
            }

            // Check for non-unique guids and non-guid formatted ids in the loaded XML file. Ignore improvements.xml since the ids are used in a different way.
            if (!objReference.DuplicatesChecked || blnHasLiveCustomData)
            {
                using (XmlNodeList xmlNodeList = objDoc.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            if (objNode.HasChildNodes)
                            {
                                // Parsing the node into an XDocument for LINQ parsing would result in slightly slower overall code (31 samples vs. 30 samples).
                                CheckIdNodes(objNode, strFileName);
                            }
                        }
                    }
                }

                objReference.DuplicatesChecked = true;
            }

            return objDoc;
        }

        public static XPathNavigator GetFastNavigator(this XmlDocument xmlDoc)
        {
            if (xmlDoc == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                xmlDoc.Save(memStream);
                memStream.Position = 0;
                using (XmlReader objXmlReader = XmlReader.Create(memStream, GlobalOptions.SafeXmlReaderSettings))
                    return new XPathDocument(objXmlReader).CreateNavigator();
            }
        }

        private static void CheckIdNodes(XmlNode xmlParentNode, string strFileName)
        {
            if (Utils.IsUnitTest)
                return;
            HashSet<string> setDuplicateIDs = new HashSet<string>();
            List<string> lstItemsWithMalformedIDs = new List<string>(1);
            // Key is ID, Value is a list of the names of all items with that ID.
            Dictionary<string, IList<string>> dicItemsWithIDs = new Dictionary<string, IList<string>>();
            CheckIdNode(xmlParentNode, setDuplicateIDs, lstItemsWithMalformedIDs, dicItemsWithIDs);

            if (setDuplicateIDs.Count > 0)
            {
                StringBuilder sbdDuplicatesNames = new StringBuilder();
                foreach (IList<string> lstDuplicateNames in dicItemsWithIDs.Where(x => setDuplicateIDs.Contains(x.Key)).Select(x => x.Value))
                {
                    if (sbdDuplicatesNames.Length != 0)
                        sbdDuplicatesNames.AppendLine();
                    sbdDuplicatesNames.AppendJoin(Environment.NewLine, lstDuplicateNames);
                }
                Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo
                    , LanguageManager.GetString("Message_DuplicateGuidWarning")
                    , setDuplicateIDs.Count
                    , strFileName
                    , sbdDuplicatesNames.ToString()));
            }

            if (lstItemsWithMalformedIDs.Count > 0)
            {
                Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo
                    , LanguageManager.GetString("Message_NonGuidIdWarning")
                    , lstItemsWithMalformedIDs.Count
                    , strFileName
                    , string.Join(Environment.NewLine, lstItemsWithMalformedIDs)));
            }
        }

        private static void CheckIdNode(XmlNode xmlParentNode, ICollection<string> setDuplicateIDs, ICollection<string> lstItemsWithMalformedIDs, IDictionary<string, IList<string>> dicItemsWithIDs)
        {
            using (XmlNodeList xmlChildNodeList = xmlParentNode.SelectNodes("*"))
            {
                if (!(xmlChildNodeList?.Count > 0))
                    return;

                foreach (XmlNode xmlLoopNode in xmlChildNodeList)
                {
                    string strId = xmlLoopNode["id"]?.InnerText.ToUpperInvariant();
                    if (!string.IsNullOrEmpty(strId))
                    {
                        if (xmlLoopNode.Name == "knowledgeskilllevel")
                            continue; //TODO: knowledgeskilllevel node in lifemodules.xml uses ids instead of name references. Find a better way to manage this!
                        string strItemName = xmlLoopNode["name"]?.InnerText
                                             ?? xmlLoopNode["stage"]?.InnerText
                                             ?? xmlLoopNode["category"]?.InnerText
                                             ?? strId;
                        if (!strId.IsGuid())
                            lstItemsWithMalformedIDs.Add(strItemName);
                        else if (dicItemsWithIDs.TryGetValue(strId, out IList<string> lstNamesList))
                        {
                            if (!setDuplicateIDs.Contains(strId))
                            {
                                setDuplicateIDs.Add(strId);
                                if (strItemName == strId)
                                    strItemName = string.Empty;
                            }

                            lstNamesList.Add(strItemName);
                        }
                        else
                            dicItemsWithIDs.Add(strId, new List<string>(2) { strItemName });
                    }

                    // Perform recursion so that nested elements that also have ids are also checked (e.g. Metavariants)
                    CheckIdNode(xmlLoopNode, setDuplicateIDs, lstItemsWithMalformedIDs, dicItemsWithIDs);
                }
            }
        }

        private static void AppendTranslations(XmlDocument xmlDataDocument, XmlNode xmlTranslationListParentNode, XmlNode xmlDataParentNode)
        {
            foreach (XmlNode objChild in xmlTranslationListParentNode.ChildNodes)
            {
                XmlNode xmlItem = null;
                string strXPathPrefix = xmlTranslationListParentNode.Name + '/' + objChild.Name + '[';
                string strChildName = objChild["id"]?.InnerText;
                if (!string.IsNullOrEmpty(strChildName))
                {
                    xmlItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + "id = " + strChildName.CleanXPath() + ']');
                }
                if (xmlItem == null)
                {
                    strChildName = objChild["name"]?.InnerText.Replace("&amp;", "&");
                    if (!string.IsNullOrEmpty(strChildName))
                    {
                        xmlItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + "name = " + strChildName.CleanXPath() + ']');
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

                    xmlLoopNode = objChild["alttexts"];
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
                        XmlElement objItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + ". = " + objChild.InnerXml.Replace("&amp;", "&").CleanXPath() + "]") as XmlElement;
                        // Expected result is null if not found.
                        objItem?.SetAttribute("translate", strTranslate);
                    }
                }
            }
        }

        private static bool DoProcessCustomDataFiles(XmlDocument xmlFile, XmlDocument xmlDataDoc, string strLoopPath, string strFileName, SearchOption eSearchOption = SearchOption.TopDirectoryOnly)
        {
            bool blnReturn = false;
            XmlElement objDocElement = xmlDataDoc.DocumentElement;
            foreach (string strFile in Directory.GetFiles(strLoopPath, "override_*_" + strFileName, eSearchOption)
                .Concat(Directory.GetFiles(strLoopPath, "override_" + strFileName, eSearchOption)))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlFile.Load(objXmlReader);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            foreach (XmlNode objType in objNode.ChildNodes)
                            {
                                StringBuilder sbdFilter = new StringBuilder();
                                XmlNode xmlIdNode = objType["id"];
                                if (xmlIdNode != null)
                                    sbdFilter.Append("id = ").Append(xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                else
                                {
                                    xmlIdNode = objType["name"];
                                    if (xmlIdNode != null)
                                        sbdFilter.Append("name = ").Append(xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                }

                                // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                                XmlNodeList objAmendingNodeExtraIds = objType.SelectNodes("child::*[@isidnode = \"True\"]");
                                if (objAmendingNodeExtraIds?.Count > 0)
                                {
                                    foreach (XmlNode objExtraId in objAmendingNodeExtraIds)
                                    {
                                        if (sbdFilter.Length > 0)
                                            sbdFilter.Append(" and ");
                                        sbdFilter.Append(objExtraId.Name).Append(" = ").Append(objExtraId.InnerText.Replace("&amp;", "&").CleanXPath());
                                    }
                                }

                                if (sbdFilter.Length > 0)
                                {
                                    sbdFilter.Insert(0, '[').Append(']')
                                        .Insert(0, "/chummer/" + objNode.Name + '/' + objType.Name);
                                    XmlNode objItem = xmlDataDoc.SelectSingleNode(sbdFilter.ToString());
                                    if (objItem != null)
                                    {
                                        objItem.InnerXml = objType.InnerXml;
                                        blnReturn = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
            foreach (string strFile in Directory.GetFiles(strLoopPath, "custom_*_" + strFileName, eSearchOption)
                .Concat(Directory.GetFiles(strLoopPath, "custom_" + strFileName, eSearchOption)))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlFile.Load(objXmlReader);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0 && objDocElement != null)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>(objNode.ChildNodes.Count);
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                XmlNode objParentNode = objChild.ParentNode;
                                if (objParentNode == null)
                                    continue;
                                StringBuilder sbdFilter = new StringBuilder();
                                XmlNode xmlIdNode = objChild["id"];
                                if (xmlIdNode != null)
                                    sbdFilter.Append("id = " + xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                XmlNode xmlNameNode = objChild["name"];
                                if (xmlNameNode != null)
                                {
                                    if (sbdFilter.Length > 0)
                                        sbdFilter.Append(" and ");
                                    sbdFilter.Append("name = ").Append(xmlNameNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                }

                                // Only do this if the child has the name or id field since this is what we must match on.
                                if (sbdFilter.Length > 0)
                                {
                                    StringBuilder sbdParentNodeFilter = new StringBuilder();
                                    if (objParentNode.Attributes?.Count > 0)
                                    {
                                        sbdParentNodeFilter.AppendJoin(" and ", objParentNode.Attributes.Cast<XmlAttribute>().Select(x =>
                                            "@" + x.Name + " = " + x.Value.Replace("&amp;", "&").CleanXPath()));
                                        if (sbdParentNodeFilter.Length > 0)
                                            sbdParentNodeFilter.Insert(0, '[').Append(']');
                                    }
                                    sbdParentNodeFilter.Insert(0, "/chummer/" + objParentNode.Name).Append('/').Append(objChild.Name);
                                    sbdFilter.Insert(0, '[').Append(']')
                                        .Insert(0, sbdParentNodeFilter.ToString());
                                    XmlNode objItem = xmlDataDoc.SelectSingleNode(sbdFilter.ToString());
                                    if (objItem != null)
                                        lstDelete.Add(objChild);
                                }
                            }

                            // Remove the offending items from the node we're about to merge in.
                            foreach (XmlNode objRemoveNode in lstDelete)
                            {
                                objNode.RemoveChild(objRemoveNode);
                            }

                            XmlNode xmlExistingNode = objDocElement[objNode.Name];
                            if (xmlExistingNode != null
                                && xmlExistingNode.Attributes?.Count == objNode.Attributes?.Count
                                && xmlExistingNode.Attributes?.Cast<XmlAttribute>().All(x => objNode.Attributes?.Cast<XmlAttribute>().Any(y => x.Name == y.Name && x.Value == y.Value) == true) != false)
                            {
                                /* We need to do this to avoid creating multiple copies of the root node, ie
                                        <chummer>
                                            <metatypes>
                                                <metatype>Standard</metatype>
                                            </metatypes>
                                            <metatypes>
                                                <metatype>Custom</metatype>
                                            </metatypes>
                                        </chummer>
                                        Otherwise xpathnavigators that to a selectsinglenode will only grab the first instance of the name. TODO: fix better?
                                    */
                                foreach (XmlNode childNode in objNode.ChildNodes)
                                {
                                    xmlExistingNode.AppendChild(xmlDataDoc.ImportNode(childNode, true));
                                }
                            }
                            else
                            {
                                // Append the entire child node to the new document.
                                objDocElement.AppendChild(xmlDataDoc.ImportNode(objNode, true));
                            }

                            blnReturn = true;
                        }
                    }
                }
            }

            // Load any amending data we might have, i.e. rules that only amend items instead of replacing them. Do not attempt this if we're loading the Improvements file.
            foreach (string strFile in Directory.GetFiles(strLoopPath, "amend_*_" + strFileName, eSearchOption)
                .Concat(Directory.GetFiles(strLoopPath, "amend_" + strFileName, eSearchOption)))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlFile.Load(objXmlReader);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            blnReturn = AmendNodeChildren(xmlDataDoc, objNode, "/chummer") || blnReturn;
                        }
                    }
                }
            }

            return blnReturn;
        }

        /// <summary>
        /// Deep search a document to amend with a new node.
        /// If Attributes exist for the amending node, the Attributes for the original node will all be overwritten.
        /// </summary>
        /// <param name="xmlDoc">Document element in which to operate.</param>
        /// <param name="xmlAmendingNode">The amending (new) node.</param>
        /// <param name="strXPath">The current XPath in the document element that leads to the target node(s) where the amending node would be applied.</param>
        /// <param name="lstExtraNodesToAddIfNotFound">List of extra nodes to add (with their XPaths) if the given amending node would be added if not found, with each entry's node being the parent of the next entry's node. Needed in case of recursing into nodes that don't exist.</param>
        /// <returns>True if any amends were made, False otherwise.</returns>
        private static bool AmendNodeChildren(XmlDocument xmlDoc, XmlNode xmlAmendingNode, string strXPath, IList<Tuple<XmlNode, string>> lstExtraNodesToAddIfNotFound = null)
        {
            bool blnReturn = false;
            StringBuilder sbdFilter = new StringBuilder();
            string strOperation = string.Empty;
            string strRegexPattern = string.Empty;
            bool blnAddIfNotFoundAttributePresent = false;
            bool blnAddIfNotFound = false;
            XmlAttributeCollection objAmendingNodeAttribs = xmlAmendingNode.Attributes;
            if (objAmendingNodeAttribs != null)
            {
                // This attribute is not used by the node itself, so it can be removed to speed up node importing later on.
                objAmendingNodeAttribs.RemoveNamedItem("isidnode");

                // Gets the custom XPath filter defined for what children to fetch. If it exists, use that as the XPath filter for targeting nodes.
                XmlNode objCustomXPath = objAmendingNodeAttribs.RemoveNamedItem("xpathfilter");
                if (objCustomXPath != null)
                {
                    sbdFilter.Append(objCustomXPath.InnerText.Replace("&amp;", "&").Replace("&quot;", "\""));
                }
                else
                {
                    // Fetch the old node based on identifiers present in the amending node (id or name)
                    XmlNode objAmendingNodeId = xmlAmendingNode["id"];
                    if (objAmendingNodeId != null)
                    {
                        sbdFilter.Append("id = ").Append(objAmendingNodeId.InnerText.Replace("&amp;", "&").CleanXPath());
                    }
                    else
                    {
                        objAmendingNodeId = xmlAmendingNode["name"];
                        if (objAmendingNodeId != null)
                        {
                            sbdFilter.Append("name = ").Append(objAmendingNodeId.InnerText.Replace("&amp;", "&").CleanXPath());
                        }
                    }
                    // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                    using (XmlNodeList xmlChildrenWithIds = xmlAmendingNode.SelectNodes("child::*[@isidnode = \"True\"]"))
                    {
                        if (xmlChildrenWithIds != null)
                        {
                            foreach (XmlNode objExtraId in xmlChildrenWithIds)
                            {
                                if (sbdFilter.Length > 0)
                                    sbdFilter.Append(" and ");
                                sbdFilter.Append(objExtraId.Name).Append(" = ").Append(objExtraId.InnerText.Replace("&amp;", "&").CleanXPath());
                            }
                        }
                    }
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
                    blnAddIfNotFoundAttributePresent = true;
                    blnAddIfNotFound = objAddIfNotFound.InnerText == bool.TrueString;
                }

                // Gets the RegEx pattern for if the node is meant to be a RegEx replace operation
                XmlNode objRegExPattern = objAmendingNodeAttribs.RemoveNamedItem("regexpattern");
                if (objRegExPattern != null)
                {
                    strRegexPattern = objRegExPattern.InnerText;
                }
            }

            if (sbdFilter.Length > 0)
                sbdFilter.Insert(0, '[').Append(']');

            // AddNode operation will always add this node in its current state.
            // This is almost the functionality of "custom_*" (exception: if a custom item already exists, it won't be replaced), but with all the extra bells and whistles of the amend system for targeting where to add the custom item
            if (strOperation == "addnode")
            {
                using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPath))
                {
                    if (xmlParentNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlParentNode in xmlParentNodeList)
                        {
                            xmlParentNode.AppendChild(xmlDoc.ImportNode(xmlAmendingNode, true));
                        }

                        blnReturn = true;
                    }
                }

                return blnReturn;
            }

            sbdFilter.Insert(0, strXPath + '/' + xmlAmendingNode.Name);
            string strNewXPath = sbdFilter.ToString();

            XmlNodeList objNodesToEdit = xmlDoc.SelectNodes(strNewXPath);

            List<XmlNode> lstElementChildren = null;
            // Pre-cache list of elements if we don't have an operation specified or have recurse specified
            if (string.IsNullOrEmpty(strOperation) || strOperation == "recurse")
            {
                lstElementChildren = new List<XmlNode>(xmlAmendingNode.ChildNodes.Count);
                if (xmlAmendingNode.HasChildNodes)
                {
                    foreach (XmlNode objChild in xmlAmendingNode.ChildNodes)
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
                    // Replace operation without "addifnotfound" offers identical functionality to "override_*", but with all the extra bells and whistles of the amend system for targeting what to override
                    // Replace operation with "addifnotfound" offers identical functionality to "custom_*", but with all the extra bells and whistles of the amend system for targeting where to replace/add the item
                case "replace":
                case "append":
                    break;
                case "regexreplace":
                    // Operation only supported if a pattern is actually defined
                    if (string.IsNullOrWhiteSpace(strRegexPattern))
                        goto case "replace";
                    // Test to make sure RegEx pattern is properly formatted before actual amend code starts
                    // Exit out early if it is not properly formatted
                    try
                    {
                        bool _ = Regex.IsMatch("Test for properly formatted Regular Expression pattern.", strRegexPattern);
                    }
                    catch (ArgumentException ex)
                    {
                        Program.MainForm.ShowMessageBox(ex.ToString());
                        return false;
                    }
                    break;
                case "recurse":
                    // Operation only supported if we have children
                    if (lstElementChildren?.Count > 0)
                        break;
                    goto default;
                // If no supported operation is specified, the default is...
                default:
                    // ..."recurse" if we have children...
                    if (lstElementChildren?.Count > 0)
                        strOperation = "recurse";
                    // ..."append" if we don't have children and there's no target...
                    else if (objNodesToEdit?.Count == 0)
                        strOperation = "append";
                    // ..."replace" but adding if not found if we don't have children and there are one or more targets.
                    else
                    {
                        strOperation = "replace";
                        if (!blnAddIfNotFoundAttributePresent)
                            blnAddIfNotFound = true;
                    }

                    break;
            }

            // We found nodes to target with the amend!
            if (objNodesToEdit?.Count > 0 || (strOperation == "recurse" && !blnAddIfNotFound))
            {
                // Recurse is special in that it doesn't directly target nodes, but does so indirectly through strNewXPath...
                if (strOperation == "recurse")
                {
                    if (lstElementChildren?.Count > 0)
                    {
                        if (!(lstExtraNodesToAddIfNotFound?.Count > 0) && objNodesToEdit?.Count > 0)
                        {
                            foreach (XmlNode objChild in lstElementChildren)
                            {
                                blnReturn = AmendNodeChildren(xmlDoc, objChild, strNewXPath);
                            }
                        }
                        else
                        {
                            if (lstExtraNodesToAddIfNotFound == null)
                                lstExtraNodesToAddIfNotFound = new List<Tuple<XmlNode, string>>(1);
                            Tuple<XmlNode, string> objMyData = new Tuple<XmlNode, string>(xmlAmendingNode, strXPath);
                            lstExtraNodesToAddIfNotFound.Add(objMyData);
                            foreach (XmlNode objChild in lstElementChildren)
                            {
                                blnReturn = AmendNodeChildren(xmlDoc, objChild, strNewXPath, lstExtraNodesToAddIfNotFound);
                            }
                            // Remove our info in case we weren't added.
                            // List is used instead of a Stack because oldest element needs to be retrieved first if an element is found
                            lstExtraNodesToAddIfNotFound.Remove(objMyData);
                        }
                    }
                }
                // ... otherwise loop through any nodes that satisfy the XPath filter.
                else
                {
                    foreach (XmlNode objNodeToEdit in objNodesToEdit)
                    {
                        XmlNode xmlParentNode = objNodeToEdit.ParentNode;
                        // If the old node exists and the amending node has the attribute 'amendoperation="remove"', then the old node is completely erased.
                        if (strOperation == "remove")
                        {
                            xmlParentNode?.RemoveChild(objNodeToEdit);
                        }
                        else
                        {
                            switch (strOperation)
                            {
                                case "append":
                                    if (xmlAmendingNode.HasChildNodes)
                                    {
                                        foreach (XmlNode xmlChild in xmlAmendingNode.ChildNodes)
                                        {
                                            XmlNodeType eChildNodeType = xmlChild.NodeType;

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
                                                            if (eChildNodeType != XmlNodeType.Attribute || objChildToEdit.Name == xmlChild.Name)
                                                            {
                                                                objChildToEdit.Value += xmlChild.Value;
                                                                blnItemFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (blnItemFound)
                                                    continue;
                                            }

                                            StripAmendAttributesRecursively(xmlChild);
                                            objNodeToEdit.AppendChild(xmlDoc.ImportNode(xmlChild, true));
                                        }
                                    }
                                    else if (objNodeToEdit.HasChildNodes)
                                    {
                                        using (XmlNodeList xmlGrandParentNodeList = xmlDoc.SelectNodes(strXPath))
                                        {
                                            if (xmlGrandParentNodeList?.Count > 0)
                                            {
                                                foreach (XmlNode xmlGrandparentNode in xmlGrandParentNodeList)
                                                {
                                                    StripAmendAttributesRecursively(xmlAmendingNode);
                                                    xmlGrandparentNode.AppendChild(xmlDoc.ImportNode(xmlAmendingNode, true));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "replace":
                                    StripAmendAttributesRecursively(xmlAmendingNode);
                                    xmlParentNode?.ReplaceChild(xmlDoc.ImportNode(xmlAmendingNode, true), objNodeToEdit);
                                    break;
                                case "regexreplace":
                                    if (xmlAmendingNode.HasChildNodes)
                                    {
                                        foreach (XmlNode xmlChild in xmlAmendingNode.ChildNodes)
                                        {
                                            XmlNodeType eChildNodeType = xmlChild.NodeType;

                                            // Text, Attributes, and CDATA are subject to the RegexReplace
                                            if (eChildNodeType == XmlNodeType.Text ||
                                                eChildNodeType == XmlNodeType.Attribute ||
                                                eChildNodeType == XmlNodeType.CDATA)
                                            {
                                                if (objNodeToEdit.HasChildNodes)
                                                {
                                                    foreach (XmlNode objChildToEdit in objNodeToEdit.ChildNodes)
                                                    {
                                                        if (objChildToEdit.NodeType == eChildNodeType)
                                                        {
                                                            if (eChildNodeType != XmlNodeType.Attribute || objChildToEdit.Name == xmlChild.Name)
                                                            {
                                                                // Try-Catch just in case initial RegEx pattern validity check overlooked something
                                                                try
                                                                {
                                                                    objChildToEdit.Value = Regex.Replace(objChildToEdit.Value, strRegexPattern, xmlChild.Value);
                                                                }
                                                                catch (ArgumentException ex)
                                                                {
                                                                    Program.MainForm.ShowMessageBox(ex.ToString());
                                                                    // If we get a RegEx parse error for the first node, we'll get it for all nodes being modified by this amend
                                                                    // So just exit out early instead of spamming the user with a bunch of error messages
                                                                    if (!blnReturn)
                                                                        return blnReturn;
                                                                }
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // If amending node has no contents, then treat it as if it just had an empty string Text data as its only content
                                    else if (objNodeToEdit.HasChildNodes)
                                    {
                                        foreach (XmlNode objChildToEdit in objNodeToEdit.ChildNodes)
                                        {
                                            if (objChildToEdit.NodeType == XmlNodeType.Text)
                                            {
                                                // Try-Catch just in case initial RegEx pattern validity check overlooked something
                                                try
                                                {
                                                    objChildToEdit.Value = Regex.Replace(objChildToEdit.Value, strRegexPattern, string.Empty);
                                                }
                                                catch (ArgumentException ex)
                                                {
                                                    Program.MainForm.ShowMessageBox(ex.ToString());
                                                    // If we get a RegEx parse error for the first node, we'll get it for all nodes being modified by this amend
                                                    // So just exit out early instead of spamming the user with a bunch of error messages
                                                    if (!blnReturn)
                                                        return blnReturn;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    blnReturn = true;
                }
            }
            // If there aren't any old nodes found and the amending node is tagged as needing to be added should this be the case, then append the entire amending node to the XPath.
            else if (strOperation == "append" || blnAddIfNotFound && (strOperation == "recurse" || strOperation == "replace"))
            {
                // Indication that we recursed into a set of nodes that don't exist in the base document, so those nodes will need to be recreated
                if (lstExtraNodesToAddIfNotFound?.Count > 0)
                {
                    // Because this is a list, foreach will move from oldest element to newest
                    // List used instead of a Queue because the youngest element needs to be retrieved first if no additions were made
                    foreach (Tuple<XmlNode, string> objDataToAdd in lstExtraNodesToAddIfNotFound)
                    {
                        using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(objDataToAdd.Item2))
                        {
                            if (xmlParentNodeList?.Count > 0)
                            {
                                foreach (XmlNode xmlParentNode in xmlParentNodeList)
                                {
                                    xmlParentNode.AppendChild(xmlDoc.ImportNode(objDataToAdd.Item1, false));
                                }
                            }
                        }
                    }

                    lstExtraNodesToAddIfNotFound.Clear(); // Everything in the list up to this point has been added, so now we clear the list
                }
                using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPath))
                {
                    if (xmlParentNodeList?.Count > 0)
                    {
                        foreach (XmlNode xmlParentNode in xmlParentNodeList)
                        {
                            StripAmendAttributesRecursively(xmlAmendingNode);
                            xmlParentNode.AppendChild(xmlDoc.ImportNode(xmlAmendingNode, true));
                        }

                        blnReturn = true;
                    }
                }
            }

            return blnReturn;
        }

        /// <summary>
        /// Strips attributes that are only used by the Amend system from a node and all of its children.
        /// </summary>
        /// <param name="xmlNodeToStrip">Node on which to operate</param>
        private static void StripAmendAttributesRecursively(XmlNode xmlNodeToStrip)
        {
            XmlAttributeCollection objAmendingNodeAttribs = xmlNodeToStrip.Attributes;
            if (objAmendingNodeAttribs?.Count > 0)
            {
                objAmendingNodeAttribs.RemoveNamedItem("isidnode");
                objAmendingNodeAttribs.RemoveNamedItem("xpathfilter");
                objAmendingNodeAttribs.RemoveNamedItem("amendoperation");
                objAmendingNodeAttribs.RemoveNamedItem("addifnotfound");
                objAmendingNodeAttribs.RemoveNamedItem("regexpattern");
            }

            if (xmlNodeToStrip.HasChildNodes)
                foreach (XmlNode xmlChildNode in xmlNodeToStrip.ChildNodes)
                    StripAmendAttributesRecursively(xmlChildNode);
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        public static void Verify(string strLanguage, IReadOnlyCollection<string> lstBooks)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return;
            XmlDocument objLanguageDoc = new XmlDocument
            {
                XmlResolver = null
            };
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string strFilePath = Path.Combine(languageDirectoryPath, strLanguage + "_data.xml");

            try
            {
                using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                        objLanguageDoc.Load(objXmlReader);
            }
            catch (IOException ex)
            {
                Program.MainForm.ShowMessageBox(ex.ToString());
                return;
            }
            catch (XmlException ex)
            {
                Program.MainForm.ShowMessageBox(ex.ToString());
                return;
            }

            XPathNavigator objLanguageNavigator = objLanguageDoc.GetFastNavigator();

            string strLangPath = Path.Combine(languageDirectoryPath, "results_" + strLanguage + ".xml");
            FileStream objStream = new FileStream(strLangPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            })
            {
                objWriter.WriteStartDocument();
                // <results>
                objWriter.WriteStartElement("results");

                string strPath = Path.Combine(Utils.GetStartupPath, "data");
                foreach (string strFile in Directory.GetFiles(strPath, "*.xml"))
                {
                    string strFileName = Path.GetFileName(strFile);

                    if (string.IsNullOrEmpty(strFileName)
                        || strFileName.StartsWith("amend_", StringComparison.OrdinalIgnoreCase)
                        || strFileName.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                        || strFileName.StartsWith("override_", StringComparison.OrdinalIgnoreCase)
                        || strFile.EndsWith("packs.xml", StringComparison.OrdinalIgnoreCase)
                        || strFile.EndsWith("lifemodules.xml", StringComparison.OrdinalIgnoreCase)
                        || strFile.EndsWith("sheets.xml", StringComparison.OrdinalIgnoreCase))
                        continue;
                    // Load the current English file.
                    XPathNavigator objEnglishDoc = Load(strFileName).GetFastNavigator();
                    XPathNavigator objEnglishRoot = objEnglishDoc.SelectSingleNode("/chummer");

                    // First pass: make sure the document exists.
                    bool blnExists = false;
                    XPathNavigator objLanguageRoot = objLanguageNavigator.SelectSingleNode("/chummer/chummer[@file = " + strFileName.CleanXPath() + "]");
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
                                    blnContinue = lstBooks.Any(strBook => strBook == xmlSource.Value);
                                }

                                if (blnContinue)
                                {
                                    if ((strTypeName == "costs"
                                         || strTypeName == "safehousecosts"
                                         || strTypeName == "comforts"
                                         || strTypeName == "neighborhoods"
                                         || strTypeName == "securities")
                                        && strFile.EndsWith("lifestyles.xml", StringComparison.OrdinalIgnoreCase))
                                        continue;
                                    if (strTypeName == "modifiers" && strFile.EndsWith("ranges.xml", StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    string strChildName = objChild.Name;
                                    XPathNavigator xmlTranslatedType = objLanguageRoot.SelectSingleNode(strTypeName);
                                    XPathNavigator xmlName = objChild.SelectSingleNode("name");
                                    // Look for a matching entry in the Language file.
                                    if (xmlName != null)
                                    {
                                        string strChildNameElement = xmlName.Value;
                                        XPathNavigator xmlNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[name = " + strChildNameElement.CleanXPath() + "]");
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

                                                if (strFile.EndsWith("mentors.xml", StringComparison.OrdinalIgnoreCase)
                                                    || strFile.EndsWith("paragons.xml", StringComparison.OrdinalIgnoreCase))
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

                                            // At least one piece of data was missing so write out the result node.
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
                                                    XPathNavigator objTranslate =
                                                        objLanguageRoot.SelectSingleNode(string.Format(GlobalOptions.InvariantCultureInfo,
                                                            "metatypes/metatype[name = {0}]/metavariants/metavariant[name = {1}]",
                                                            strChildNameElement.CleanXPath(), strMetavariantName.CleanXPath()));
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
                                    else if (strFile.EndsWith("tips.xml", StringComparison.OrdinalIgnoreCase))
                                    {
                                        XPathNavigator xmlText = objChild.SelectSingleNode("text");
                                        // Look for a matching entry in the Language file.
                                        if (xmlText != null)
                                        {
                                            string strChildTextElement = xmlText.Value;
                                            XPathNavigator xmlNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[text = " + strChildTextElement.CleanXPath() + "]");
                                            if (xmlNode != null)
                                            {
                                                // A match was found, so see what elements, if any, are missing.
                                                bool blnTranslate = xmlNode.SelectSingleNode("translate") != null || xmlNode.SelectSingleNode("@translated")?.Value == bool.TrueString;

                                                // At least one piece of data was missing so write out the result node.
                                                if (!blnTranslate)
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        objWriter.WriteStartElement(strTypeName);
                                                    }

                                                    // <results>
                                                    objWriter.WriteStartElement(strChildName);
                                                    objWriter.WriteElementString("text", strChildTextElement);
                                                    if (!blnTranslate)
                                                        objWriter.WriteElementString("missing", "translate");
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
                                                objWriter.WriteElementString("text", strChildTextElement);
                                                // </result>
                                                objWriter.WriteEndElement();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string strChildInnerText = objChild.Value;
                                        if (!string.IsNullOrEmpty(strChildInnerText))
                                        {
                                            // The item does not have a name which means it should have a translate CharacterAttribute instead.
                                            XPathNavigator objNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[text() =" + strChildInnerText.CleanXPath() + "]");
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
                                    XPathNavigator objNode = objEnglishRoot.SelectSingleNode(string.Format(GlobalOptions.InvariantCultureInfo,
                                        "/chummer/{0}/{1}[name = {2}]",
                                        objType.Name, strChildName, strChildNameElement.CleanXPath()));
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

                // </results>
                objWriter.WriteEndElement();
                objWriter.WriteEndDocument();
            }
        }
        #endregion
    }
}
