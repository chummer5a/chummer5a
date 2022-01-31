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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using NLog;

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
            private readonly object _loadingLock = new object();

            /// <summary>
            /// Whether or not the XML content has been successfully checked for duplicate guids.
            /// </summary>
            public bool DuplicatesChecked { get; set; } = Utils.IsUnitTest;

            private XmlDocument _xmlContent = new XmlDocument { XmlResolver = null };

            /// <summary>
            /// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public XmlDocument XmlContent
            {
                get => _xmlContent;
                set
                {
                    lock (_loadingLock)
                    {
                        if (value == _xmlContent)
                            return;
                        IsLoaded = false;
                        _xmlContent = value;
                        if (value != null)
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                value.Save(memStream);
                                memStream.Position = 0;
                                using (XmlReader objXmlReader = XmlReader.Create(memStream, GlobalSettings.SafeXmlReaderSettings))
                                    XPathContent = new XPathDocument(objXmlReader);
                            }
                        }
                        else
                            XPathContent = null;
                        IsLoaded = true;
                    }
                }
            }

            /// <summary>
            /// XmlContent, but in a form that is much faster to navigate
            /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
            /// </summary>
            public XPathDocument XPathContent { get; private set; }

            /// <summary>
            /// Whether the Reference has finished loading its content
            /// </summary>
            public bool IsLoaded { get; set; }
        }

        private static readonly LockingDictionary<KeyArray<string>, XmlReference> s_DicXmlDocuments =
            new LockingDictionary<KeyArray<string>, XmlReference>(); // Key is language + array of all file paths for the complete combination of data used
        private static bool s_blnSetDataDirectoriesLoaded = true;
        private static readonly object s_SetDataDirectoriesLock = new object();
        private static readonly HashSet<string> s_SetDataDirectories = new HashSet<string>(Path
            .Combine(Utils.GetStartupPath, "data").Yield()
            .Concat(GlobalSettings.CustomDataDirectoryInfos.Select(x => x.DirectoryPath)));
        private static readonly Dictionary<string, HashSet<string>> s_DicPathsWithCustomFiles = new Dictionary<string, HashSet<string>>();

        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        #region Methods

        static XmlManager()
        {
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;
            foreach (string strFileName in Utils.BasicDataFileNames)
            {
                if (!s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setLoop))
                {
                    setLoop = new HashSet<string>();
                    s_DicPathsWithCustomFiles.Add(strFileName, setLoop);
                }
                else
                    setLoop.Clear();
                setLoop.AddRange(CompileRelevantCustomDataPaths(strFileName, s_SetDataDirectories));
            }
        }

        public static void RebuildDataDirectoryInfo(IEnumerable<CustomDataDirectoryInfo> customDirectories)
        {
            if (!s_blnSetDataDirectoriesLoaded)
                return;
            s_DicXmlDocuments.Clear();
            lock (s_SetDataDirectoriesLock)
            {
                s_blnSetDataDirectoriesLoaded = false;
                s_SetDataDirectories.Clear();
                s_SetDataDirectories.Add(Path.Combine(Utils.GetStartupPath, "data"));
                foreach (CustomDataDirectoryInfo objCustomDataDirectory in customDirectories)
                {
                    s_SetDataDirectories.Add(objCustomDataDirectory.DirectoryPath);
                }
                if (!Utils.IsDesignerMode && !Utils.IsRunningInVisualStudio)
                {
                    foreach (string strFileName in Utils.BasicDataFileNames)
                    {
                        if (!s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setLoop))
                        {
                            setLoop = new HashSet<string>();
                            s_DicPathsWithCustomFiles.Add(strFileName, setLoop);
                        }
                        else
                            setLoop.Clear();
                        setLoop.AddRange(CompileRelevantCustomDataPaths(strFileName, s_SetDataDirectories));
                    }
                }
                s_blnSetDataDirectoriesLoaded = true;
            }
        }

        /// <summary>
        /// Load the selected XML file and its associated custom files synchronously.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// Returns a new XPathNavigator associated with the XPathDocument so that multiple threads each get their own navigator if they're called on the same file
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathNavigator LoadXPath(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            return LoadXPathCoreAsync(true, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load the selected XML file and its associated custom files asynchronously.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// Returns a new XPathNavigator associated with the XPathDocument so that multiple threads each get their own navigator if they're called on the same file
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        public static Task<XPathNavigator> LoadXPathAsync(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            return LoadXPathCoreAsync(false, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile);
        }

        /// <summary>
        /// Core of the method to load an XML file and its associated custom file.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <returns></returns>
        private static async Task<XPathNavigator> LoadXPathCoreAsync(bool blnSync, string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            bool blnFileFound = false;
            string strPath = string.Empty;
            strFileName = Path.GetFileName(strFileName);
            while (!s_blnSetDataDirectoriesLoaded) // Wait to make sure our data directories are loaded before proceeding
            {
                if (blnSync)
                    Utils.SafeSleep();
                else
                    await Utils.SafeSleepAsync();
            }
            foreach (string strDirectory in s_SetDataDirectories)
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
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;

            string[] astrRelevantCustomDataPaths;
            if (lstEnabledCustomDataPaths == null)
                astrRelevantCustomDataPaths = Array.Empty<string>();
            else if (s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setDirectoriesPossible))
                astrRelevantCustomDataPaths = lstEnabledCustomDataPaths.Where(x => setDirectoriesPossible.Contains(x)).ToArray();
            else
                astrRelevantCustomDataPaths = CompileRelevantCustomDataPaths(strFileName, lstEnabledCustomDataPaths).ToArray();
            List<string> lstKey = new List<string>(2 + astrRelevantCustomDataPaths.Length) {strLanguage, strPath};
            lstKey.AddRange(astrRelevantCustomDataPaths);
            KeyArray<string> objDataKey = new KeyArray<string>(lstKey);

            // Look to see if this XmlDocument is already loaded.
            XmlDocument xmlDocumentOfReturn = null;
            if (blnLoadFile
                || (GlobalSettings.LiveCustomData && strFileName != "improvements.xml")
                || !s_DicXmlDocuments.TryGetValue(objDataKey, out XmlReference xmlReferenceOfReturn))
            {
                // The file was not found in the reference list, so it must be loaded.
                xmlReferenceOfReturn = null;
                bool blnLoadSuccess;
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    xmlDocumentOfReturn = Load(strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile);
                    blnLoadSuccess = s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn);
                }
                else
                    blnLoadSuccess = await LoadAsync(strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile)
                    .ContinueWith(
                        x =>
                        {
                            xmlDocumentOfReturn = x.Result;
                            return s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn);
                        });
                if (!blnLoadSuccess)
                {
                    Utils.BreakIfDebug();
                    return null;
                }
            }
            // Live custom data will cause the reference's document to not be the same as the actual one we need, so we'll need to remake the document returned by the Load
            if (GlobalSettings.LiveCustomData && strFileName != "improvements.xml" && xmlDocumentOfReturn != null)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    xmlDocumentOfReturn.Save(memStream);
                    memStream.Position = 0;
                    using (XmlReader objXmlReader = XmlReader.Create(memStream, GlobalSettings.SafeXmlReaderSettings))
                        return new XPathDocument(objXmlReader).CreateNavigator();
                }
            }
            while (!xmlReferenceOfReturn.IsLoaded) // Wait for the reference to get loaded
            {
                if (blnSync)
                    Utils.SafeSleep();
                else
                    await Utils.SafeSleepAsync();
            }

            return xmlReferenceOfReturn.XPathContent.CreateNavigator();
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file synchronously.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [Annotations.NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlDocument Load(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            return LoadCoreAsync(true, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file asynchronously.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [Annotations.NotNull]
        public static Task<XmlDocument> LoadAsync(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            return LoadCoreAsync(false, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile);
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file asynchronously.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [Annotations.NotNull]
        private static async Task<XmlDocument> LoadCoreAsync(bool blnSync, string strFileName, IReadOnlyCollection<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false)
        {
            bool blnFileFound = false;
            string strPath = string.Empty;
            strFileName = Path.GetFileName(strFileName);
            while (!s_blnSetDataDirectoriesLoaded) // Wait to make sure our data directories are loaded before proceeding
            {
                if (blnSync)
                    Utils.SafeSleep();
                else
                    await Utils.SafeSleepAsync();
            }

            foreach (string strDirectory in s_SetDataDirectories)
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
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;

            string[] astrRelevantCustomDataPaths;
            if (lstEnabledCustomDataPaths == null)
                astrRelevantCustomDataPaths = Array.Empty<string>();
            else if (s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setDirectoriesPossible))
                astrRelevantCustomDataPaths = lstEnabledCustomDataPaths.Where(x => setDirectoriesPossible.Contains(x)).ToArray();
            else
                astrRelevantCustomDataPaths = CompileRelevantCustomDataPaths(strFileName, lstEnabledCustomDataPaths).ToArray();
            bool blnHasCustomData = astrRelevantCustomDataPaths.Length > 0;
            List<string> lstKey = new List<string>(2 + astrRelevantCustomDataPaths.Length) { strLanguage, strPath };
            lstKey.AddRange(astrRelevantCustomDataPaths);
            KeyArray<string> objDataKey = new KeyArray<string>(lstKey);

            XmlDocument xmlReturn = null;
            // Create a new document that everything will be merged into.
            XmlDocument xmlScratchpad = new XmlDocument { XmlResolver = null };
            // Look to see if this XmlDocument is already loaded.
            if (!s_DicXmlDocuments.TryGetValue(objDataKey, out XmlReference xmlReferenceOfReturn))
            {
                int intEmergencyRelease = 0;
                xmlReferenceOfReturn = new XmlReference();
                // We break either when we successfully add our XmlReference to the dictionary or when we end up successfully fetching an existing one.
                for (; intEmergencyRelease <= 1000; ++intEmergencyRelease)
                {
                    // The file was not found in the reference list, so it must be loaded.
                    if (s_DicXmlDocuments.TryAdd(objDataKey, xmlReferenceOfReturn))
                    {
                        blnLoadFile = true;
                        break;
                    }
                    // It somehow got added in the meantime, so let's fetch it again
                    if (s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn))
                        break;
                    // We're iterating the loop because we failed to get the reference, so we need to re-allocate our reference because it was in an out-argument above
                    xmlReferenceOfReturn = new XmlReference();
                }
                if (intEmergencyRelease > 1000) // Shouldn't ever happen, but just in case it does, emergency exit out of the loading function
                {
                    Utils.BreakIfDebug();
                    return new XmlDocument { XmlResolver = null };
                }
            }

            if (blnLoadFile)
            {
                xmlReferenceOfReturn.IsLoaded = false;
                if (blnHasCustomData)
                {
                    // If we have any custom data, make sure the base data is already loaded so we can easily just copy it over
                    XmlDocument xmlBaseDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? Load(strFileName, null, strLanguage)
                        : await LoadAsync(strFileName, null, strLanguage).ConfigureAwait(false);
                    xmlReturn = xmlBaseDocument.Clone() as XmlDocument;
                }
                else if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    // When loading in non-English data, just clone the English stuff instead of recreating it to hopefully save on time
                    XmlDocument xmlBaseDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? Load(strFileName, null, GlobalSettings.DefaultLanguage)
                        : await LoadAsync(strFileName, null, GlobalSettings.DefaultLanguage).ConfigureAwait(false);
                    xmlReturn = xmlBaseDocument.Clone() as XmlDocument;
                }
                if (xmlReturn == null) // Not an else in case something goes wrong in safe cast in the line above
                {
                    xmlReturn = new XmlDocument { XmlResolver = null };
                    // write the root chummer node.
                    xmlReturn.AppendChild(xmlReturn.CreateElement("chummer"));
                    XmlElement xmlReturnDocElement = xmlReturn.DocumentElement;
                    // Load the base file and retrieve all of the child nodes.
                    try
                    {
                        xmlScratchpad.LoadStandard(strPath);

                        if (xmlReturnDocElement != null)
                        {
                            using (XmlNodeList xmlNodeList = xmlScratchpad.SelectNodes("/chummer/*"))
                            {
                                if (xmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode objNode in xmlNodeList)
                                    {
                                        // Append the entire child node to the new document.
                                        xmlReturnDocElement.AppendChild(xmlReturn.ImportNode(objNode, true));
                                    }
                                }
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Log.Info(e);
                        Utils.BreakIfDebug();
                    }
                    catch (XmlException e)
                    {
                        Log.Warn(e);
                        Utils.BreakIfDebug();
                    }
                }

                // Load any override data files the user might have. Do not attempt this if we're loading the Improvements file.
                if (blnHasCustomData)
                {
                    foreach (string strLoopPath in astrRelevantCustomDataPaths)
                    {
                        DoProcessCustomDataFiles(xmlScratchpad, xmlReturn, strLoopPath, strFileName);
                    }
                }

                // Load the translation file for the current base data file if the selected language is not en-us.
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    // Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
                    // The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
                    XPathDocument objDataDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? LanguageManager.GetDataDocument(strLanguage)
                        : await LanguageManager.GetDataDocumentAsync(strLanguage);
                    if (objDataDoc != null)
                    {
                        XmlNode xmlBaseChummerNode = xmlReturn.SelectSingleNode("/chummer");
                        foreach (XPathNavigator objType in objDataDoc.CreateNavigator().Select("/chummer/chummer[@file = " + strFileName.CleanXPath() + "]/*"))
                        {
                            AppendTranslations(xmlReturn, objType, xmlBaseChummerNode);
                        }
                    }
                }

                // Cache the merged document and its relevant information (also sets IsLoaded to true).
                xmlReferenceOfReturn.XmlContent = xmlReturn;
                // Make sure we do not override the cached document with our live data
                if (GlobalSettings.LiveCustomData && blnHasCustomData)
                    xmlReturn = xmlReferenceOfReturn.XmlContent.Clone() as XmlDocument;
            }
            else
            {
                while (!xmlReferenceOfReturn.IsLoaded) // Wait for the reference to get loaded
                {
                    if (blnSync)
                        Utils.SafeSleep();
                    else
                        await Utils.SafeSleepAsync();
                }
                // Make sure we do not override the cached document with our live data
                if (GlobalSettings.LiveCustomData && blnHasCustomData)
                    xmlReturn = xmlReferenceOfReturn.XmlContent.Clone() as XmlDocument;
                else
                    xmlReturn = xmlReferenceOfReturn.XmlContent;
            }

            xmlReturn = xmlReturn ?? new XmlDocument { XmlResolver = null };
            if (strFileName == "improvements.xml")
                return xmlReturn;

            // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
            bool blnHasLiveCustomData = false;
            if (GlobalSettings.LiveCustomData)
            {
                strPath = Path.Combine(Utils.GetStartupPath, "livecustomdata");
                if (Directory.Exists(strPath))
                {
                    blnHasLiveCustomData = DoProcessCustomDataFiles(xmlScratchpad, xmlReturn, strPath, strFileName);
                }
            }

            // Check for non-unique guids and non-guid formatted ids in the loaded XML file. Ignore improvements.xml since the ids are used in a different way.
            if (!xmlReferenceOfReturn.DuplicatesChecked || blnHasLiveCustomData)
            {
                xmlReferenceOfReturn.DuplicatesChecked = true; // Set early to make sure work isn't done multiple times in case of multiple threads
                using (XmlNodeList xmlNodeList = xmlReturn.SelectNodes("/chummer/*"))
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
            }

            return xmlReturn;
        }

        private static void CheckIdNodes(XmlNode xmlParentNode, string strFileName)
        {
            if (Utils.IsUnitTest)
                return;
            List<string> lstItemsWithMalformedIDs = new List<string>(1);
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setDuplicateIDs))
            {
                // Key is ID, Value is a list of the names of all items with that ID.
                Dictionary<string, IList<string>> dicItemsWithIDs = new Dictionary<string, IList<string>>();
                CheckIdNode(xmlParentNode, setDuplicateIDs, lstItemsWithMalformedIDs, dicItemsWithIDs);

                if (setDuplicateIDs.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDuplicatesNames))
                    {
                        foreach (IList<string> lstDuplicateNames in dicItemsWithIDs
                                                                    .Where(x => setDuplicateIDs.Contains(x.Key))
                                                                    .Select(x => x.Value))
                        {
                            if (sbdDuplicatesNames.Length != 0)
                                sbdDuplicatesNames.AppendLine();
                            sbdDuplicatesNames.AppendJoin(Environment.NewLine, lstDuplicateNames);
                        }

                        Program.MainForm?.ShowMessageBox(string.Format(GlobalSettings.CultureInfo
                                                                       , LanguageManager.GetString(
                                                                           "Message_DuplicateGuidWarning")
                                                                       , setDuplicateIDs.Count
                                                                       , strFileName
                                                                       , sbdDuplicatesNames.ToString()));
                    }
                }
            }

            if (lstItemsWithMalformedIDs.Count > 0)
            {
                Program.MainForm?.ShowMessageBox(string.Format(GlobalSettings.CultureInfo
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
                    string strId = xmlLoopNode["id"]?.InnerText;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        strId = strId.ToUpperInvariant();
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

        private static void AppendTranslations(XmlDocument xmlDataDocument, XPathNavigator xmlTranslationListParentNode, XmlNode xmlDataParentNode)
        {
            foreach (XPathNavigator objChild in xmlTranslationListParentNode.SelectAndCacheExpression("*"))
            {
                XmlNode xmlItem = null;
                string strXPathPrefix = xmlTranslationListParentNode.Name + '/' + objChild.Name + '[';
                string strChildName = objChild.SelectSingleNodeAndCacheExpression("id")?.Value;
                if (!string.IsNullOrEmpty(strChildName))
                {
                    xmlItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + "id = " + strChildName.CleanXPath() + ']');
                }
                if (xmlItem == null)
                {
                    strChildName = objChild.SelectSingleNodeAndCacheExpression("name")?.Value.Replace("&amp;", "&");
                    if (!string.IsNullOrEmpty(strChildName))
                    {
                        xmlItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + "name = " + strChildName.CleanXPath() + ']');
                    }
                }
                // If this is a translatable item, find the proper node and add/update this information.
                if (xmlItem != null)
                {
                    XPathNavigator xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("translate");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altpage");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altcode");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altnotes");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altadvantage");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altdisadvantage");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("altnameonpage");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    xmlLoopNode = objChild.SelectSingleNodeAndCacheExpression("alttexts");
                    if (xmlLoopNode != null)
                        xmlItem.AppendChild(xmlLoopNode.ToXmlNode(xmlDataDocument));

                    string strTranslate = objChild.SelectSingleNodeAndCacheExpression("@translate")?.InnerXml;
                    if (!string.IsNullOrEmpty(strTranslate))
                    {
                        // Handle Category name translations.
                        (xmlItem as XmlElement)?.SetAttribute("translate", strTranslate);
                    }

                    // Sub-children to also process with the translation
                    XPathNavigator xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("specs");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("metavariants");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("choices");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("talents");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("versions");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem);
                    }
                }
                else
                {
                    string strTranslate = objChild.SelectSingleNodeAndCacheExpression("@translate")?.InnerXml;
                    if (!string.IsNullOrEmpty(strTranslate))
                    {
                        // Handle Category name translations.
                        XmlElement objItem = xmlDataParentNode.SelectSingleNode(strXPathPrefix + ". = " + objChild.InnerXml.Replace("&amp;", "&").CleanXPath() + ']') as XmlElement;
                        // Expected result is null if not found.
                        objItem?.SetAttribute("translate", strTranslate);
                    }
                }
            }
        }

        /// <summary>
        /// Filter through a list of data paths and return a list of ones that modify the given file. Used to recycle data files between different rule sets.
        /// </summary>
        /// <param name="strFileName">Name of the file that would be modified by custom data files.</param>
        /// <param name="lstPaths">Paths to check for custom data files relevant to <paramref name="strFileName"/>.</param>
        /// <returns>A list of paths with <paramref name="lstPaths"/> that is relevant to <paramref name="strFileName"/>, in the same order that they are in <paramref name="lstPaths"/>.</returns>
        private static IEnumerable<string> CompileRelevantCustomDataPaths(string strFileName, IEnumerable<string> lstPaths)
        {
            if (strFileName == "improvements.xml" || lstPaths == null)
                yield break;
            foreach (string strLoopPath in lstPaths)
            {
                if (Directory.EnumerateFiles(strLoopPath, "*_" + strFileName, SearchOption.AllDirectories)
                             .Any(x =>
                             {
                                 string strInnerFileName = Path.GetFileName(x);
                                 return strInnerFileName.StartsWith("override_") || strInnerFileName.StartsWith("custom_")
                                     || strInnerFileName.StartsWith("amend_");
                             }))
                {
                    yield return strLoopPath;
                }
            }
        }

        private static bool DoProcessCustomDataFiles(XmlDocument xmlFile, XmlDocument xmlDataDoc, string strLoopPath, string strFileName, SearchOption eSearchOption = SearchOption.AllDirectories)
        {
            bool blnReturn = false;
            XmlElement objDocElement = xmlDataDoc.DocumentElement;
            List<string> lstPossibleCustomFiles = Directory.GetFiles(strLoopPath, "*_" + strFileName, eSearchOption).ToList();
            foreach (string strFile in lstPossibleCustomFiles)
            {
                if (!Path.GetFileName(strFile).StartsWith("override_"))
                    continue;
                try
                {
                    xmlFile.LoadStandard(strFile);
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
                                string strFilter;
                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdFilter))
                                {
                                    XmlNode xmlIdNode = objType["id"];
                                    if (xmlIdNode != null)
                                        sbdFilter.Append("id = ")
                                                 .Append(xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                    else
                                    {
                                        xmlIdNode = objType["name"];
                                        if (xmlIdNode != null)
                                            sbdFilter.Append("name = ")
                                                     .Append(xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath());
                                    }

                                    // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                                    using (XmlNodeList objAmendingNodeExtraIds
                                           = objType.SelectNodes("child::*[@isidnode = " + bool.TrueString.CleanXPath() + ']'))
                                    {
                                        if (objAmendingNodeExtraIds?.Count > 0)
                                        {
                                            foreach (XmlNode objExtraId in objAmendingNodeExtraIds)
                                            {
                                                if (sbdFilter.Length > 0)
                                                    sbdFilter.Append(" and ");
                                                sbdFilter.Append(objExtraId.Name).Append(" = ")
                                                         .Append(
                                                             objExtraId.InnerText.Replace("&amp;", "&").CleanXPath());
                                            }
                                        }
                                    }

                                    strFilter = sbdFilter.ToString();
                                }
                                if (!string.IsNullOrEmpty(strFilter))
                                {
                                    XmlNode objItem = xmlDataDoc.SelectSingleNode(
                                        "/chummer/" + objNode.Name + '/' + objType.Name + '[' + strFilter + ']');
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
            foreach (string strFile in lstPossibleCustomFiles)
            {
                if (!Path.GetFileName(strFile).StartsWith("custom_"))
                    continue;
                try
                {
                    xmlFile.LoadStandard(strFile);
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
                                string strFilter = string.Empty;
                                XmlNode xmlIdNode = objChild["id"];
                                if (xmlIdNode != null)
                                    strFilter = "id = " + xmlIdNode.InnerText.Replace("&amp;", "&").CleanXPath();
                                else
                                {
                                    XmlNode xmlNameNode = objChild["name"];
                                    if (xmlNameNode != null)
                                    {
                                        strFilter += (string.IsNullOrEmpty(strFilter)
                                                         ? "name = "
                                                         : " and name = ") +
                                                     xmlNameNode.InnerText.Replace("&amp;", "&").CleanXPath();
                                    }
                                }

                                // Only do this if the child has the name or id field since this is what we must match on.
                                if (!string.IsNullOrEmpty(strFilter))
                                {
                                    string strParentNodeFilter = string.Empty;
                                    if (objParentNode.Attributes?.Count > 0)
                                    {
                                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                   out StringBuilder sbdParentNodeFilter))
                                        {
                                            foreach (XmlAttribute objLoopAttribute in objParentNode.Attributes)
                                            {
                                                sbdParentNodeFilter
                                                    .Append('@').Append(objLoopAttribute.Name).Append(" = ")
                                                    .Append(objLoopAttribute.Value.Replace("&amp;", "&")
                                                                            .CleanXPath()).Append(" and ");
                                            }

                                            if (sbdParentNodeFilter.Length > 0)
                                                sbdParentNodeFilter.Length -= 5;

                                            strParentNodeFilter = sbdParentNodeFilter.ToString();
                                        }
                                    }
                                    XmlNode objItem = xmlDataDoc.SelectSingleNode(string.IsNullOrEmpty(strParentNodeFilter)
                                        ? "/chummer/" + objParentNode.Name + '/' + objChild.Name + '[' + strFilter + ']'
                                        : "/chummer/" + objParentNode.Name + '[' + strParentNodeFilter + "]/" + objChild.Name + '[' + strFilter + ']');
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
                                && xmlExistingNode.Attributes?.Count == objNode.Attributes?.Count)
                            {
                                bool blnAllMatching = true;
                                foreach (XmlAttribute x in xmlExistingNode.Attributes)
                                {
                                    if (objNode.Attributes.GetNamedItem(x.Name)?.Value != x.Value)
                                    {
                                        blnAllMatching = false;
                                        break;
                                    }
                                }

                                if (blnAllMatching)
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
            foreach (string strFile in lstPossibleCustomFiles)
            {
                if (!Path.GetFileName(strFile).StartsWith("amend_"))
                    continue;
                try
                {
                    xmlFile.LoadStandard(strFile);
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
            string strFilter = string.Empty;
            string strOperation = string.Empty;
            string strRegexPattern = string.Empty;
            bool blnAddIfNotFoundAttributePresent = false;
            bool blnAddIfNotFound = false;
            XmlAttributeCollection objAmendingNodeAttribs = xmlAmendingNode.Attributes;
            if (objAmendingNodeAttribs != null)
            {
                // This attribute is not used by the node itself, so it can be removed to speed up node importing later on.
                objAmendingNodeAttribs.RemoveNamedItem("isidnode");

                // Gets the specific operation to execute on this node.
                XmlNode objAmendOperation = objAmendingNodeAttribs.RemoveNamedItem("amendoperation");
                if (objAmendOperation != null)
                {
                    strOperation = objAmendOperation.InnerText;
                }

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
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
                            sbdFilter.Append("id = ")
                                     .Append(objAmendingNodeId.InnerText.Replace("&amp;", "&").CleanXPath());
                        }
                        else
                        {
                            objAmendingNodeId = xmlAmendingNode["name"];
                            if (objAmendingNodeId != null && (strOperation == "remove"
                                                              || xmlAmendingNode.SelectSingleNode(
                                                                  "child::*[not(self::name)]") != null))
                            {
                                // A few places in the data files use just "name" as an actual entry in a list, so only default to using it as an id node
                                // if there are other nodes present in the amending node or if a remove operation is specified (since that only requires an id node).
                                sbdFilter.Append("name = ")
                                         .Append(objAmendingNodeId.InnerText.Replace("&amp;", "&").CleanXPath());
                            }
                        }

                        // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                        using (XmlNodeList xmlChildrenWithIds
                               = xmlAmendingNode.SelectNodes("child::*[@isidnode = " + bool.TrueString.CleanXPath() + ']'))
                        {
                            if (xmlChildrenWithIds != null)
                            {
                                foreach (XmlNode objExtraId in xmlChildrenWithIds)
                                {
                                    if (sbdFilter.Length > 0)
                                        sbdFilter.Append(" and ");
                                    sbdFilter.Append(objExtraId.Name).Append(" = ")
                                             .Append(objExtraId.InnerText.Replace("&amp;", "&").CleanXPath());
                                }
                            }
                        }
                    }

                    if (sbdFilter.Length > 0)
                        strFilter = '[' + sbdFilter.ToString() + ']';
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

            string strNewXPath = strXPath + '/' + xmlAmendingNode.Name + strFilter;

            using (XmlNodeList objNodesToEdit = xmlDoc.SelectNodes(strNewXPath))
            {
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
                            bool _ = Regex.IsMatch("Test for properly formatted Regular Expression pattern.",
                                strRegexPattern);
                        }
                        catch (ArgumentException ex)
                        {
                            Program.MainForm?.ShowMessageBox(ex.ToString());
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
                                Tuple<XmlNode, string> objMyData =
                                    new Tuple<XmlNode, string>(xmlAmendingNode, strXPath);
                                lstExtraNodesToAddIfNotFound.Add(objMyData);
                                foreach (XmlNode objChild in lstElementChildren)
                                {
                                    blnReturn = AmendNodeChildren(xmlDoc, objChild, strNewXPath,
                                        lstExtraNodesToAddIfNotFound);
                                }

                                // Remove our info in case we weren't added.
                                // List is used instead of a Stack because oldest element needs to be retrieved first if an element is found
                                lstExtraNodesToAddIfNotFound.Remove(objMyData);
                            }
                        }
                    }
                    // ... otherwise loop through any nodes that satisfy the XPath filter.
                    else if (objNodesToEdit != null)
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

                                                switch (eChildNodeType)
                                                {
                                                    // Skip adding comments, they're pointless for the purposes of Chummer5a's code
                                                    case XmlNodeType.Comment:
                                                        continue;
                                                    // Text, Attributes, and CDATA should add their values to existing children of the same type if possible
                                                    case XmlNodeType.Text:
                                                    case XmlNodeType.Attribute:
                                                    case XmlNodeType.CDATA:
                                                    {
                                                        if (objNodeToEdit.HasChildNodes)
                                                        {
                                                            XmlNode objChildToEdit = null;
                                                            if (eChildNodeType == XmlNodeType.Attribute)
                                                            {
                                                                foreach (XmlNode objLoopChildNode in objNodeToEdit
                                                                             .ChildNodes)
                                                                {
                                                                    if (objLoopChildNode.NodeType == eChildNodeType
                                                                        && objLoopChildNode.Name == xmlChild.Name)
                                                                    {
                                                                        objChildToEdit = objLoopChildNode;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                foreach (XmlNode objLoopChildNode in objNodeToEdit
                                                                             .ChildNodes)
                                                                {
                                                                    if (objLoopChildNode.NodeType == eChildNodeType)
                                                                    {
                                                                        objChildToEdit = objLoopChildNode;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            if (objChildToEdit != null)
                                                            {
                                                                objChildToEdit.Value += xmlChild.Value;
                                                                continue;
                                                            }
                                                        }

                                                        break;
                                                    }
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
                                                        xmlGrandparentNode.AppendChild(
                                                            xmlDoc.ImportNode(xmlAmendingNode, true));
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "replace":
                                        StripAmendAttributesRecursively(xmlAmendingNode);
                                        xmlParentNode?.ReplaceChild(xmlDoc.ImportNode(xmlAmendingNode, true),
                                            objNodeToEdit);
                                        break;
                                    case "regexreplace":
                                        if (xmlAmendingNode.HasChildNodes)
                                        {
                                            foreach (XmlNode xmlChild in xmlAmendingNode.ChildNodes)
                                            {
                                                XmlNodeType eChildNodeType = xmlChild.NodeType;

                                                // Text, Attributes, and CDATA are subject to the RegexReplace
                                                if ((eChildNodeType == XmlNodeType.Text ||
                                                     eChildNodeType == XmlNodeType.Attribute ||
                                                     eChildNodeType == XmlNodeType.CDATA) && objNodeToEdit.HasChildNodes)
                                                {
                                                    foreach (XmlNode objChildToEdit in objNodeToEdit.ChildNodes)
                                                    {
                                                        if (objChildToEdit.NodeType == eChildNodeType && (eChildNodeType != XmlNodeType.Attribute ||
                                                            objChildToEdit.Name == xmlChild.Name))
                                                        {
                                                            // Try-Catch just in case initial RegEx pattern validity check overlooked something
                                                            try
                                                            {
                                                                objChildToEdit.Value =
                                                                    Regex.Replace(objChildToEdit.Value,
                                                                        strRegexPattern, xmlChild.Value);
                                                            }
                                                            catch (ArgumentException ex)
                                                            {
                                                                Program.MainForm?.ShowMessageBox(ex.ToString());
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
                                                        objChildToEdit.Value = Regex.Replace(objChildToEdit.Value,
                                                            strRegexPattern, string.Empty);
                                                    }
                                                    catch (ArgumentException ex)
                                                    {
                                                        Program.MainForm?.ShowMessageBox(ex.ToString());
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

                        // Handle the special case where we're modifying multiple nodes and want to add contents to parents if they're not found, but some parents do still contain existing values
                        if (blnAddIfNotFound)
                        {
                            using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPath))
                            {
                                if (xmlParentNodeList?.Count > objNodesToEdit.Count)
                                {
                                    StripAmendAttributesRecursively(xmlAmendingNode);
                                    foreach (XmlNode xmlParentNode in xmlParentNodeList)
                                    {
                                        // Make sure we can't actually find any targets
                                        if (xmlParentNode.SelectSingleNode(xmlAmendingNode.Name + strFilter) == null)
                                            xmlParentNode.AppendChild(xmlDoc.ImportNode(xmlAmendingNode, true));
                                    }
                                }
                            }
                        }

                        blnReturn = true;
                    }
                }
                // If there aren't any old nodes found and the amending node is tagged as needing to be added should this be the case, then append the entire amending node to the XPath.
                else if (strOperation == "append" ||
                         (blnAddIfNotFound && (strOperation == "recurse" || strOperation == "replace")))
                {
                    // Indication that we recursed into a set of nodes that don't exist in the base document, so those nodes will need to be recreated
                    if (lstExtraNodesToAddIfNotFound?.Count > 0 && string.IsNullOrEmpty(strFilter)) // Filter of any kind on this node would fail after addition, so skip if there is one
                    {
                        // Because this is a list, foreach will move from oldest element to newest
                        // List used instead of a Queue because the youngest element needs to be retrieved first if no additions were made
                        foreach ((XmlNode xmlNodeToAdd, string strXPathToAdd) in lstExtraNodesToAddIfNotFound)
                        {
                            using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPathToAdd))
                            {
                                if (!(xmlParentNodeList?.Count > 0))
                                    continue;
                                foreach (XmlNode xmlParentNode in xmlParentNodeList)
                                {
                                    xmlParentNode.AppendChild(xmlDoc.ImportNode(xmlNodeToAdd, false));
                                }
                            }
                        }

                        lstExtraNodesToAddIfNotFound
                            .Clear(); // Everything in the list up to this point has been added, so now we clear the list
                    }

                    using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPath))
                    {
                        if (xmlParentNodeList?.Count > 0)
                        {
                            StripAmendAttributesRecursively(xmlAmendingNode);
                            foreach (XmlNode xmlParentNode in xmlParentNodeList)
                            {
                                xmlParentNode.AppendChild(xmlDoc.ImportNode(xmlAmendingNode, true));
                            }

                            blnReturn = true;
                        }
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

        public static bool AnyXslFiles(string strLanguage, IEnumerable<Character> lstCharacters = null)
        {
            GetXslFilesFromLocalDirectory(strLanguage, out bool blnReturn, out List<ListItem> _, lstCharacters, false, false);
            return blnReturn;
        }

        public static List<ListItem> GetXslFilesFromLocalDirectory(string strLanguage,
                                                                   IEnumerable<Character> lstCharacters = null, bool blnUsePool = false)
        {
            GetXslFilesFromLocalDirectory(strLanguage, out bool _, out List<ListItem> lstReturn, lstCharacters, true, blnUsePool);
            return lstReturn;
        }

        private static void GetXslFilesFromLocalDirectory(string strLanguage, out bool blnAnyItem, out List<ListItem> lstSheets, IEnumerable<Character> lstCharacters, bool blnDoList, bool blnUsePool)
        {
            blnAnyItem = false;
            HashSet<string> setAddedSheetFileNames = blnDoList ? Utils.StringHashSetPool.Get() : null;
            try
            {
                if (lstCharacters != null)
                {
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(10);
                    else
                        lstSheets = null;
                    // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
                    foreach (Character objCharacter in lstCharacters)
                    {
                        foreach (XPathNavigator xmlSheet in objCharacter.LoadDataXPath("sheets.xml", strLanguage)
                                                                        .SelectAndCacheExpression(
                                                                            "/chummer/sheets[@lang="
                                                                            + strLanguage.CleanXPath()
                                                                            + "]/sheet[not(hide)]"))
                        {
                            string strSheetFileName = xmlSheet.SelectSingleNodeAndCacheExpression("filename")?.Value;
                            if (string.IsNullOrEmpty(strSheetFileName))
                                continue;
                            if (!blnDoList)
                            {
                                blnAnyItem = true;
                                return;
                            }

                            if (!setAddedSheetFileNames.Add(strSheetFileName))
                                continue;
                            blnAnyItem = true;
                            lstSheets.Add(new ListItem(
                                              !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                                  StringComparison.OrdinalIgnoreCase)
                                                  ? Path.Combine(strLanguage, strSheetFileName)
                                                  : strSheetFileName,
                                              xmlSheet.SelectSingleNodeAndCacheExpression("name")?.Value
                                              ?? LanguageManager.GetString("String_Unknown")));
                        }
                    }
                }
                else
                {
                    XPathNodeIterator xmlIterator = LoadXPath("sheets.xml", null, strLanguage)
                        .SelectAndCacheExpression(
                            "/chummer/sheets[@lang=" + strLanguage.CleanXPath() + "]/sheet[not(hide)]");
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(xmlIterator.Count);
                    else
                        lstSheets = null;

                    foreach (XPathNavigator xmlSheet in xmlIterator)
                    {
                        string strSheetFileName = xmlSheet.SelectSingleNodeAndCacheExpression("filename")?.Value;
                        if (string.IsNullOrEmpty(strSheetFileName))
                            continue;
                        if (!blnDoList)
                        {
                            blnAnyItem = true;
                            return;
                        }

                        if (!setAddedSheetFileNames.Add(strSheetFileName))
                            continue;
                        blnAnyItem = true;
                        lstSheets.Add(new ListItem(
                                          !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                              StringComparison.OrdinalIgnoreCase)
                                              ? Path.Combine(strLanguage, strSheetFileName)
                                              : strSheetFileName,
                                          xmlSheet.SelectSingleNodeAndCacheExpression("name")?.Value
                                          ?? LanguageManager.GetString("String_Unknown")));
                    }
                }
            }
            finally
            {
                if (setAddedSheetFileNames != null)
                    Utils.StringHashSetPool.Return(setAddedSheetFileNames);
            }
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        public static void Verify(string strLanguage, ICollection<string> lstBooks)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return;
            XPathDocument objLanguageDoc;
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string strFilePath = Path.Combine(languageDirectoryPath, strLanguage + "_data.xml");

            try
            {
                using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                        objLanguageDoc = new XPathDocument(objXmlReader);
            }
            catch (IOException ex)
            {
                Program.MainForm?.ShowMessageBox(ex.ToString());
                return;
            }
            catch (XmlException ex)
            {
                Program.MainForm?.ShowMessageBox(ex.ToString());
                return;
            }

            XPathNavigator objLanguageNavigator = objLanguageDoc.CreateNavigator();

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

                    // First pass: make sure the document exists.
                    bool blnExists = false;
                    XPathNavigator objLanguageRoot = objLanguageNavigator.SelectSingleNode("/chummer/chummer[@file = " + strFileName.CleanXPath() + ']');
                    if (objLanguageRoot != null)
                        blnExists = true;

                    // <file name="x" needstobeadded="y">
                    objWriter.WriteStartElement("file");
                    objWriter.WriteAttributeString("name", strFileName);

                    if (blnExists)
                    {
                        // Load the current English file.
                        XPathNavigator objEnglishDoc = LoadXPath(strFileName);
                        XPathNavigator objEnglishRoot = objEnglishDoc.SelectSingleNodeAndCacheExpression("/chummer");

                        foreach (XPathNavigator objType in objEnglishRoot.SelectChildren(XPathNodeType.Element))
                        {
                            string strTypeName = objType.Name;
                            bool blnTypeWritten = false;
                            foreach (XPathNavigator objChild in objType.SelectChildren(XPathNodeType.Element))
                            {
                                // If the Node has a source element, check it and see if it's in the list of books that were specified.
                                // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                                bool blnContinue = true;
                                XPathNavigator xmlSource = objChild.SelectSingleNodeAndCacheExpression("source");
                                if (xmlSource != null)
                                {
                                    blnContinue = lstBooks.Contains(xmlSource.Value);
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
                                    XPathNavigator xmlName = objChild.SelectSingleNodeAndCacheExpression("name");
                                    // Look for a matching entry in the Language file.
                                    if (xmlName != null)
                                    {
                                        string strChildNameElement = xmlName.Value;
                                        XPathNavigator xmlNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[name = " + strChildNameElement.CleanXPath() + ']');
                                        if (xmlNode != null)
                                        {
                                            // A match was found, so see what elements, if any, are missing.
                                            bool blnTranslate = false;
                                            bool blnAltPage = false;
                                            bool blnAdvantage = false;
                                            bool blnDisadvantage = false;

                                            if (objChild.HasChildren)
                                            {
                                                if (xmlNode.SelectSingleNodeAndCacheExpression("translate") != null)
                                                    blnTranslate = true;

                                                // Do not mark page as missing if the original does not have it.
                                                if (objChild.SelectSingleNodeAndCacheExpression("page") != null)
                                                {
                                                    if (xmlNode.SelectSingleNodeAndCacheExpression("altpage") != null)
                                                        blnAltPage = true;
                                                }
                                                else
                                                    blnAltPage = true;

                                                if (strFile.EndsWith("mentors.xml", StringComparison.OrdinalIgnoreCase)
                                                    || strFile.EndsWith("paragons.xml", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (xmlNode.SelectSingleNodeAndCacheExpression("altadvantage") != null)
                                                        blnAdvantage = true;
                                                    if (xmlNode.SelectSingleNodeAndCacheExpression("altdisadvantage") != null)
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
                                                if (xmlNode.SelectSingleNodeAndCacheExpression("@translate") != null)
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
                                            XPathNavigator xmlMetavariants = objChild.SelectSingleNodeAndCacheExpression("metavariants");
                                            if (xmlMetavariants != null)
                                            {
                                                foreach (XPathNavigator objMetavariant in xmlMetavariants.SelectAndCacheExpression("metavariant"))
                                                {
                                                    string strMetavariantName = objMetavariant.SelectSingleNodeAndCacheExpression("name").Value;
                                                    XPathNavigator objTranslate =
                                                        objLanguageRoot.SelectSingleNode(
                                                            "metatypes/metatype[name = "
                                                            + strChildNameElement.CleanXPath()
                                                            + "]/metavariants/metavariant[name = "
                                                            + strMetavariantName.CleanXPath() + ']');
                                                    if (objTranslate != null)
                                                    {
                                                        bool blnTranslate = objTranslate.SelectSingleNodeAndCacheExpression("translate") != null;
                                                        bool blnAltPage = objTranslate.SelectSingleNodeAndCacheExpression("altpage") != null;

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
                                        XPathNavigator xmlText = objChild.SelectSingleNodeAndCacheExpression("text");
                                        // Look for a matching entry in the Language file.
                                        if (xmlText != null)
                                        {
                                            string strChildTextElement = xmlText.Value;
                                            XPathNavigator xmlNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[text = " + strChildTextElement.CleanXPath() + ']');
                                            if (xmlNode != null)
                                            {
                                                // A match was found, so see what elements, if any, are missing.
                                                bool blnTranslate = xmlNode.SelectSingleNodeAndCacheExpression("translate") != null || xmlNode.SelectSingleNodeAndCacheExpression("@translated")?.Value == bool.TrueString;

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
                                            XPathNavigator objNode = xmlTranslatedType?.SelectSingleNode(strChildName + "[. =" + strChildInnerText.CleanXPath() + ']');
                                            if (objNode != null)
                                            {
                                                // Make sure the translate attribute is populated.
                                                if (objNode.SelectSingleNodeAndCacheExpression("@translate") == null)
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
                                string strChildNameElement = objChild.SelectSingleNodeAndCacheExpression("name")?.Value;
                                // Look for a matching entry in the English file.
                                if (!string.IsNullOrEmpty(strChildNameElement))
                                {
                                    string strChildName = objChild.Name;
                                    XPathNavigator objNode = objEnglishRoot.SelectSingleNode(
                                        "/chummer/" + objType.Name + '/' + strChildName + "[name = "
                                        + strChildNameElement.CleanXPath() + ']');
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
