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
using System.Threading;
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
        private sealed class XmlReference : IHasLockObject
        {
            /// <summary>
            /// Whether or not the XML content has been successfully checked for duplicate guids.
            /// </summary>
            public bool GetDuplicatesChecked(CancellationToken token = default)
            {
                using (EnterReadLock.Enter(LockObject, token))
                    return _blnDuplicatesChecked;
            }

            public void SetDuplicatesChecked(bool blnNewValue, CancellationToken token = default)
            {
                using (EnterReadLock.Enter(LockObject, token))
                {
                    if (_blnDuplicatesChecked == blnNewValue)
                        return;
                    using (LockObject.EnterWriteLock(token))
                        _blnDuplicatesChecked = blnNewValue;
                }
            }

            /// <summary>
            /// Whether or not the XML content has been successfully checked for duplicate guids.
            /// </summary>
            public async ValueTask<bool> GetDuplicatesCheckedAsync(CancellationToken token = default)
            {
                using (await EnterReadLock.EnterAsync(LockObject, token))
                    return _blnDuplicatesChecked;
            }

            public async ValueTask SetDuplicatesCheckedAsync(bool blnNewValue, CancellationToken token = default)
            {
                using (await EnterReadLock.EnterAsync(LockObject, token))
                {
                    if (_blnDuplicatesChecked == blnNewValue)
                        return;
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token);
                    try
                    {
                        _blnDuplicatesChecked = blnNewValue;
                    }
                    finally
                    {
                        await objLocker.DisposeAsync();
                    }
                }
            }

            private XmlDocument _xmlContent = new XmlDocument { XmlResolver = null };
            private XPathDocument _objXPathContent;
            private bool _blnDuplicatesChecked = Utils.IsUnitTest;
            private bool _blnInitialLoadComplete;

            /// <summary>
            /// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public XmlDocument GetXmlContent(CancellationToken token = default)
            {
                token.ThrowIfCancellationRequested();
                while (true)
                {
                    bool blnLoadComplete;
                    using (EnterReadLock.Enter(LockObject, token))
                        blnLoadComplete = _blnInitialLoadComplete;
                    if (blnLoadComplete)
                        break;
                    Utils.SafeSleep(token);
                }
                using (EnterReadLock.Enter(LockObject, token))
                    return _xmlContent;
            }

            /// <summary>
            /// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public async ValueTask<XmlDocument> GetXmlContentAsync(CancellationToken token = default)
            {
                token.ThrowIfCancellationRequested();
                while (true)
                {
                    bool blnLoadComplete;
                    using (await EnterReadLock.EnterAsync(LockObject, token))
                        blnLoadComplete = _blnInitialLoadComplete;
                    if (blnLoadComplete)
                        break;
                    await Utils.SafeSleepAsync(token);
                }
                using (await EnterReadLock.EnterAsync(LockObject, token))
                    return _xmlContent;
            }

            /// <summary>
            /// Set the XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public void SetXmlContent(XmlDocument objContent, CancellationToken token = default)
            {
                using (EnterReadLock.Enter(LockObject, token))
                {
                    if (objContent == _xmlContent)
                        return;
                    using (LockObject.EnterWriteLock(token))
                    {
                        _xmlContent = objContent;
                        if (objContent != null)
                        {
                            _blnInitialLoadComplete = true;
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                objContent.Save(memStream);
                                memStream.Position = 0;
                                using (XmlReader objXmlReader
                                       = XmlReader.Create(memStream, GlobalSettings.SafeXmlReaderSettings))
                                    _objXPathContent = new XPathDocument(objXmlReader);
                            }
                        }
                        else
                            _objXPathContent = null;
                    }
                }
            }

            /// <summary>
            /// Set the XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
            /// </summary>
            public async ValueTask SetXmlContentAsync(XmlDocument objContent, CancellationToken token = default)
            {
                using (await EnterReadLock.EnterAsync(LockObject, token))
                {
                    if (objContent == _xmlContent)
                        return;
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token);
                    try
                    {
                        _xmlContent = objContent;
                        if (objContent != null)
                        {
                            _blnInitialLoadComplete = true;
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                objContent.Save(memStream);
                                memStream.Position = 0;
                                using (XmlReader objXmlReader
                                       = XmlReader.Create(memStream, GlobalSettings.SafeXmlReaderSettings))
                                    _objXPathContent = new XPathDocument(objXmlReader);
                            }
                        }
                        else
                            _objXPathContent = null;
                    }
                    finally
                    {
                        await objLocker.DisposeAsync();
                    }
                }
            }

            /// <summary>
            /// XmlContent, but in a form that is much faster to navigate
            /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
            /// </summary>
            public XPathDocument GetXPathContent(CancellationToken token = default)
            {
                while (true)
                {
                    bool blnLoadComplete;
                    using (EnterReadLock.Enter(LockObject, token))
                        blnLoadComplete = _blnInitialLoadComplete;
                    if (blnLoadComplete)
                        break;
                    Utils.SafeSleep(token);
                }
                using (EnterReadLock.Enter(LockObject, token))
                    return _objXPathContent;
            }

            /// <summary>
            /// XmlContent, but in a form that is much faster to navigate
            /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
            /// </summary>
            public async ValueTask<XPathDocument> GetXPathContentAsync(CancellationToken token = default)
            {
                while (true)
                {
                    bool blnLoadComplete;
                    using (await EnterReadLock.EnterAsync(LockObject, token))
                        blnLoadComplete = _blnInitialLoadComplete;
                    if (blnLoadComplete)
                        break;
                    await Utils.SafeSleepAsync(token);
                }
                using (await EnterReadLock.EnterAsync(LockObject, token))
                    return _objXPathContent;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                LockObject.Dispose();
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                return LockObject.DisposeAsync();
            }

            /// <inheritdoc />
            public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
        }

        private static readonly LockingDictionary<KeyArray<string>, XmlReference> s_DicXmlDocuments =
            new LockingDictionary<KeyArray<string>, XmlReference>(); // Key is language + array of all file paths for the complete combination of data used
        private static readonly AsyncFriendlyReaderWriterLock s_objDataDirectoriesLock = new AsyncFriendlyReaderWriterLock();
        private static readonly HashSet<string> s_SetDataDirectories = new HashSet<string>(Utils.GetDataFolderPath.Yield());
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

        public static void RebuildDataDirectoryInfo(IEnumerable<CustomDataDirectoryInfo> lstCustomDirectories, CancellationToken token = default)
        {
            using (s_objDataDirectoriesLock.EnterWriteLock(token))
            {
                token.ThrowIfCancellationRequested();
                foreach (XmlReference objReference in s_DicXmlDocuments.Values)
                    objReference.Dispose();
                token.ThrowIfCancellationRequested();
                s_DicXmlDocuments.Clear();
                token.ThrowIfCancellationRequested();
                s_SetDataDirectories.Clear();
                token.ThrowIfCancellationRequested();
                s_SetDataDirectories.Add(Utils.GetDataFolderPath);
                foreach (CustomDataDirectoryInfo objCustomDataDirectory in lstCustomDirectories)
                {
                    token.ThrowIfCancellationRequested();
                    s_SetDataDirectories.Add(objCustomDataDirectory.DirectoryPath);
                }

                if (!Utils.IsDesignerMode && !Utils.IsRunningInVisualStudio)
                {
                    foreach (string strFileName in Utils.BasicDataFileNames)
                    {
                        token.ThrowIfCancellationRequested();
                        if (!s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setLoop))
                        {
                            setLoop = new HashSet<string>();
                            s_DicPathsWithCustomFiles.Add(strFileName, setLoop);
                        }
                        else
                            setLoop.Clear();
                        token.ThrowIfCancellationRequested();
                        setLoop.AddRange(CompileRelevantCustomDataPaths(strFileName, s_SetDataDirectories, token));
                    }
                }
            }
        }

        public static async ValueTask RebuildDataDirectoryInfoAsync(IEnumerable<CustomDataDirectoryInfo> lstCustomDirectories, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await s_objDataDirectoriesLock.EnterWriteLockAsync(token);
            try
            {
                await s_DicXmlDocuments.ForEachAsync(async kvpDocument => await kvpDocument.Value.DisposeAsync(), token: token);
                await s_DicXmlDocuments.ClearAsync(token);
                s_SetDataDirectories.Clear();
                token.ThrowIfCancellationRequested();
                s_SetDataDirectories.Add(Utils.GetDataFolderPath);
                token.ThrowIfCancellationRequested();
                foreach (CustomDataDirectoryInfo objCustomDataDirectory in lstCustomDirectories)
                {
                    token.ThrowIfCancellationRequested();
                    s_SetDataDirectories.Add(objCustomDataDirectory.DirectoryPath);
                }

                if (!Utils.IsDesignerMode && !Utils.IsRunningInVisualStudio)
                {
                    foreach (string strFileName in Utils.BasicDataFileNames)
                    {
                        token.ThrowIfCancellationRequested();
                        if (!s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setLoop))
                        {
                            setLoop = new HashSet<string>();
                            s_DicPathsWithCustomFiles.Add(strFileName, setLoop);
                        }
                        else
                            setLoop.Clear();
                        token.ThrowIfCancellationRequested();
                        setLoop.AddRange(CompileRelevantCustomDataPaths(strFileName, s_SetDataDirectories, token));
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync();
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
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathNavigator LoadXPath(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            return LoadXPathCoreAsync(true, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false).GetAwaiter().GetResult();
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
        /// <param name="token">Cancellation token to use.</param>
        public static Task<XPathNavigator> LoadXPathAsync(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            return LoadXPathCoreAsync(false, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token);
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
        /// <param name="token">Cancellation token to use.</param>
        private static async Task<XPathNavigator> LoadXPathCoreAsync(bool blnSync, string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            strFileName = Path.GetFileName(strFileName);
            // Wait to make sure our data directories are loaded before proceeding
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(s_objDataDirectoriesLock, token) : await EnterReadLock.EnterAsync(s_objDataDirectoriesLock, token).ConfigureAwait(false))
            {
                if (string.IsNullOrEmpty(strLanguage))
                    strLanguage = GlobalSettings.Language;

                string strPath;
                if (Utils.BasicDataFileNames.Contains(strFileName))
                    strPath = Path.Combine(Utils.GetDataFolderPath, strFileName);
                else
                {
                    strPath = FetchBaseFileFromCustomDataPaths(strFileName, lstEnabledCustomDataPaths, token);
                    if (string.IsNullOrEmpty(strPath))
                    {
                        // We don't actually have such a file
                        Utils.BreakIfDebug();
                        return new XmlDocument {XmlResolver = null}.CreateNavigator();
                    }
                }

                string[] astrRelevantCustomDataPaths;
                if (lstEnabledCustomDataPaths == null)
                    astrRelevantCustomDataPaths = Array.Empty<string>();
                else if (s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setDirectoriesPossible))
                    astrRelevantCustomDataPaths = lstEnabledCustomDataPaths
                                                  .Where(x => setDirectoriesPossible.Contains(x)).ToArray();
                else
                {
                    astrRelevantCustomDataPaths = CompileRelevantCustomDataPaths(strFileName, lstEnabledCustomDataPaths, token)
                        .ToArray();
                    if (astrRelevantCustomDataPaths.Length > 0 && !Utils.IsDesignerMode
                                                               && !Utils.IsRunningInVisualStudio)
                    {
                        IDisposable objLocker = null;
                        IAsyncDisposable objLockerAsync = null;
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            objLocker = s_objDataDirectoriesLock.EnterWriteLock(token);
                        else
                            objLockerAsync = await s_objDataDirectoriesLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (!s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setLoop))
                            {
                                setLoop = new HashSet<string>();
                                s_DicPathsWithCustomFiles.Add(strFileName, setLoop);
                            }

                            setLoop.AddRange(astrRelevantCustomDataPaths);
                        }
                        finally
                        {
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverload
                                objLocker.Dispose();
                            else
                                await objLockerAsync.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                List<string> lstKey = new List<string>(2 + astrRelevantCustomDataPaths.Length) {strLanguage, strPath};
                lstKey.AddRange(astrRelevantCustomDataPaths);
                KeyArray<string> objDataKey = new KeyArray<string>(lstKey);

                // Look to see if this XmlDocument is already loaded.
                XmlDocument xmlDocumentOfReturn = null;
                XmlReference xmlReferenceOfReturn = null;
                bool blnDoLoad = blnLoadFile || (GlobalSettings.LiveCustomData && strFileName != "improvements.xml");
                if (!blnDoLoad)
                {
                    if (blnSync)
                        blnDoLoad = !s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn);
                    else
                    {
                        bool blnLoadSuccess;
                        (blnLoadSuccess, xmlReferenceOfReturn) = await s_DicXmlDocuments.TryGetValueAsync(objDataKey, token).ConfigureAwait(false);
                        blnDoLoad = !blnLoadSuccess;
                    }
                }

                if (blnDoLoad)
                {
                    // The file was not found in the reference list, so it must be loaded.
                    bool blnLoadSuccess;
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        xmlDocumentOfReturn = Load(strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token);
                        blnLoadSuccess = s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn);
                    }
                    else
                    {
                        xmlDocumentOfReturn
                            = await LoadAsync(strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false);
                        // Need this conditional so that we actually await the line above before we check to see if the load was successful or not
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (xmlDocumentOfReturn != null)
                        {
                            (blnLoadSuccess, xmlReferenceOfReturn)
                                = await s_DicXmlDocuments.TryGetValueAsync(objDataKey, token).ConfigureAwait(false);
                        }
                        else
                        {
                            xmlReferenceOfReturn = null;
                            blnLoadSuccess = false;
                        }
                    }

                    if (!blnLoadSuccess)
                    {
                        Utils.BreakIfDebug();
                        return null;
                    }
                }

                // Live custom data will cause the reference's document to not be the same as the actual one we need, so we'll need to remake the document returned by the Load
                if (GlobalSettings.LiveCustomData && strFileName != "improvements.xml" && xmlDocumentOfReturn != null)
                {
                    token.ThrowIfCancellationRequested();
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        xmlDocumentOfReturn.Save(memStream);
                        memStream.Position = 0;
                        using (XmlReader objXmlReader
                               = XmlReader.Create(memStream, GlobalSettings.SafeXmlReaderSettings))
                            return new XPathDocument(objXmlReader).CreateNavigator();
                    }
                }

                XPathDocument objTemp = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? xmlReferenceOfReturn.GetXPathContent(token)
                    : await xmlReferenceOfReturn.GetXPathContentAsync(token).ConfigureAwait(false);

                return objTemp.CreateNavigator();
            }
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file synchronously.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        [Annotations.NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlDocument Load(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            return LoadCoreAsync(true, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load the selected XML file and its associated custom file asynchronously.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="lstEnabledCustomDataPaths">List of enabled custom data directory paths in their load order</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        public static Task<XmlDocument> LoadAsync(string strFileName, IReadOnlyList<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            return LoadCoreAsync(false, strFileName, lstEnabledCustomDataPaths, strLanguage, blnLoadFile, token);
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
        /// <param name="token">CancellationToken to use.</param>
        private static async Task<XmlDocument> LoadCoreAsync(bool blnSync, string strFileName, IReadOnlyCollection<string> lstEnabledCustomDataPaths = null, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            bool blnFileFound = false;
            string strPath = string.Empty;
            strFileName = Path.GetFileName(strFileName);
            // Wait to make sure our data directories are loaded before proceeding
            using (blnSync
                       // ReSharper disable once MethodHasAsyncOverload
                       ? EnterReadLock.Enter(s_objDataDirectoriesLock, token)
                       : await EnterReadLock.EnterAsync(s_objDataDirectoriesLock, token).ConfigureAwait(false))
            {
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
                    return new XmlDocument {XmlResolver = null};
                }

                if (string.IsNullOrEmpty(strLanguage))
                    strLanguage = GlobalSettings.Language;

                string[] astrRelevantCustomDataPaths;
                if (lstEnabledCustomDataPaths == null)
                    astrRelevantCustomDataPaths = Array.Empty<string>();
                else if (s_DicPathsWithCustomFiles.TryGetValue(strFileName, out HashSet<string> setDirectoriesPossible))
                    astrRelevantCustomDataPaths = lstEnabledCustomDataPaths
                                                  .Where(x => setDirectoriesPossible.Contains(x)).ToArray();
                else
                    astrRelevantCustomDataPaths = CompileRelevantCustomDataPaths(strFileName, lstEnabledCustomDataPaths, token)
                        .ToArray();
                bool blnHasCustomData = astrRelevantCustomDataPaths.Length > 0;
                List<string> lstKey = new List<string>(2 + astrRelevantCustomDataPaths.Length) {strLanguage, strPath};
                lstKey.AddRange(astrRelevantCustomDataPaths);
                KeyArray<string> objDataKey = new KeyArray<string>(lstKey);

                XmlDocument xmlReturn = null;
                // Create a new document that everything will be merged into.
                XmlDocument xmlScratchpad = new XmlDocument {XmlResolver = null};
                // Look to see if this XmlDocument is already loaded.
                XmlReference xmlReferenceOfReturn;
                if (blnSync)
                {
                    // ReSharper disable MethodHasAsyncOverload
                    if (!s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn))
                    {
                        int intEmergencyRelease = 0;
                        xmlReferenceOfReturn = new XmlReference();
                        // We break either when we successfully add our XmlReference to the dictionary or when we end up successfully fetching an existing one.
                        for (; intEmergencyRelease <= Utils.SleepEmergencyReleaseMaxTicks; ++intEmergencyRelease)
                        {
                            // The file was not found in the reference list, so it must be loaded.
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            if (s_DicXmlDocuments.TryAdd(objDataKey, xmlReferenceOfReturn))
                            {
                                blnLoadFile = true;
                                break;
                            }

                            xmlReferenceOfReturn.Dispose();

                            // It somehow got added in the meantime, so let's fetch it again
                            if (s_DicXmlDocuments.TryGetValue(objDataKey, out xmlReferenceOfReturn))
                                break;
                            // We're iterating the loop because we failed to get the reference, so we need to re-allocate our reference because it was in an out-argument above
                            xmlReferenceOfReturn = new XmlReference();
                        }

                        if (intEmergencyRelease >
                            Utils
                                .SleepEmergencyReleaseMaxTicks) // Shouldn't ever happen, but just in case it does, emergency exit out of the loading function
                        {
                            Utils.BreakIfDebug();
                            xmlReferenceOfReturn.Dispose();
                            return new XmlDocument {XmlResolver = null};
                        }
                    }
                    // ReSharper restore MethodHasAsyncOverload
                }
                else
                {
                    bool blnSuccess;
                    (blnSuccess, xmlReferenceOfReturn) = await s_DicXmlDocuments.TryGetValueAsync(objDataKey, token).ConfigureAwait(false);
                    if (!blnSuccess)
                    {
                        int intEmergencyRelease = 0;
                        xmlReferenceOfReturn = new XmlReference();
                        // We break either when we successfully add our XmlReference to the dictionary or when we end up successfully fetching an existing one.
                        for (; intEmergencyRelease <= Utils.SleepEmergencyReleaseMaxTicks; ++intEmergencyRelease)
                        {
                            // The file was not found in the reference list, so it must be loaded.
                            if (await s_DicXmlDocuments.TryAddAsync(objDataKey, xmlReferenceOfReturn, token).ConfigureAwait(false))
                            {
                                blnLoadFile = true;
                                break;
                            }

                            await xmlReferenceOfReturn.DisposeAsync().ConfigureAwait(false);

                            // It somehow got added in the meantime, so let's fetch it again
                            (blnSuccess, xmlReferenceOfReturn)
                                = await s_DicXmlDocuments.TryGetValueAsync(objDataKey, token).ConfigureAwait(false);
                            if (blnSuccess)
                                break;
                            // We're iterating the loop because we failed to get the reference, so we need to re-allocate our reference because it was in an out-argument above
                            xmlReferenceOfReturn = new XmlReference();
                        }

                        if (intEmergencyRelease >
                            Utils
                                .SleepEmergencyReleaseMaxTicks) // Shouldn't ever happen, but just in case it does, emergency exit out of the loading function
                        {
                            Utils.BreakIfDebug();
                            await xmlReferenceOfReturn.DisposeAsync().ConfigureAwait(false);
                            return new XmlDocument {XmlResolver = null};
                        }
                    }
                }

                if (blnLoadFile)
                {
                    IDisposable objLocker = null;
                    IAsyncDisposable objLockerAsync = null;
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        objLocker = xmlReferenceOfReturn.LockObject.EnterWriteLock(token);
                    else
                        objLockerAsync = await xmlReferenceOfReturn.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (blnHasCustomData)
                        {
                            // If we have any custom data, make sure the base data is already loaded so we can easily just copy it over
                            XmlDocument xmlBaseDocument = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? Load(strFileName, null, strLanguage, token: token)
                                : await LoadAsync(strFileName, null, strLanguage, token: token).ConfigureAwait(false);
                            xmlReturn = xmlBaseDocument.Clone() as XmlDocument;
                        }
                        else if (!strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                     StringComparison.OrdinalIgnoreCase))
                        {
                            // When loading in non-English data, just clone the English stuff instead of recreating it to hopefully save on time
                            XmlDocument xmlBaseDocument = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? Load(strFileName, null, GlobalSettings.DefaultLanguage, token: token)
                                : await LoadAsync(strFileName, null, GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false);
                            xmlReturn = xmlBaseDocument.Clone() as XmlDocument;
                        }

                        if (xmlReturn
                            == null) // Not an else in case something goes wrong in safe cast in the line above
                        {
                            xmlReturn = new XmlDocument {XmlResolver = null};
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
                                DoProcessCustomDataFiles(xmlScratchpad, xmlReturn, strLoopPath, strFileName, token: token);
                            }
                        }

                        // Load the translation file for the current base data file if the selected language is not en-us.
                        if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        {
                            // Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
                            // The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
                            XPathDocument objDataDoc = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetDataDocument(strLanguage, token)
                                : await LanguageManager.GetDataDocumentAsync(strLanguage, token).ConfigureAwait(false);
                            if (objDataDoc != null)
                            {
                                XmlNode xmlBaseChummerNode = xmlReturn.SelectSingleNode("/chummer");
                                foreach (XPathNavigator objType in objDataDoc.CreateNavigator()
                                                                             .Select("/chummer/chummer[@file = "
                                                                                 + strFileName.CleanXPath() + "]/*"))
                                {
                                    AppendTranslations(xmlReturn, objType, xmlBaseChummerNode, token);
                                }
                            }
                        }

                        // Cache the merged document and its relevant information
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            xmlReferenceOfReturn.SetXmlContent(xmlReturn, token);
                        else
                            await xmlReferenceOfReturn.SetXmlContentAsync(xmlReturn, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            objLocker.Dispose();
                        else
                            await objLockerAsync.DisposeAsync().ConfigureAwait(false);
                    }

                    // Make sure we do not override the cached document with our live data
                    if (GlobalSettings.LiveCustomData && blnHasCustomData)
                    {
                        XmlDocument objTemp = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? xmlReferenceOfReturn.GetXmlContent(token)
                            : await xmlReferenceOfReturn.GetXmlContentAsync(token).ConfigureAwait(false);
                        xmlReturn = objTemp.Clone() as XmlDocument;
                    }
                }
                else
                {
                    XmlDocument objTemp = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? xmlReferenceOfReturn.GetXmlContent(token)
                        : await xmlReferenceOfReturn.GetXmlContentAsync(token).ConfigureAwait(false);
                    // Make sure we do not override the cached document with our live data
                    if (GlobalSettings.LiveCustomData && blnHasCustomData)
                        xmlReturn = objTemp.Clone() as XmlDocument;
                    else
                        xmlReturn = objTemp;
                }

                xmlReturn = xmlReturn ?? new XmlDocument {XmlResolver = null};
                if (strFileName == "improvements.xml")
                    return xmlReturn;

                // Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
                bool blnHasLiveCustomData = false;
                if (GlobalSettings.LiveCustomData)
                {
                    strPath = Path.Combine(Utils.GetStartupPath, "livecustomdata");
                    if (Directory.Exists(strPath))
                    {
                        blnHasLiveCustomData = DoProcessCustomDataFiles(xmlScratchpad, xmlReturn, strPath, strFileName, token: token);
                    }
                }

                // Check for non-unique guids and non-guid formatted ids in the loaded XML file. Ignore improvements.xml since the ids are used in a different way.
                if (blnHasLiveCustomData || (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? !xmlReferenceOfReturn.GetDuplicatesChecked(token)
                        : !await xmlReferenceOfReturn.GetDuplicatesCheckedAsync(token).ConfigureAwait(false)))
                {
                    // Set early to make sure work isn't done multiple times in case of multiple threads
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        xmlReferenceOfReturn.SetDuplicatesChecked(true, token);
                    else
                        await xmlReferenceOfReturn.SetDuplicatesCheckedAsync(true, token).ConfigureAwait(false);
                    using (XmlNodeList xmlNodeList = xmlReturn.SelectNodes("/chummer/*"))
                    {
                        if (xmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objNode in xmlNodeList)
                            {
                                if (objNode.HasChildNodes)
                                {
                                    // Parsing the node into an XDocument for LINQ parsing would result in slightly slower overall code (31 samples vs. 30 samples).
                                    CheckIdNodes(objNode, strFileName, token);
                                }
                            }
                        }
                    }
                }

                return xmlReturn;
            }
        }

        private static void CheckIdNodes(XmlNode xmlParentNode, string strFileName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Utils.IsUnitTest)
                return;
            List<string> lstItemsWithMalformedIDs = new List<string>(1);
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setDuplicateIDs))
            {
                // Key is ID, Value is a list of the names of all items with that ID.
                Dictionary<string, IList<string>> dicItemsWithIDs = new Dictionary<string, IList<string>>(1);
                CheckIdNode(xmlParentNode, setDuplicateIDs, lstItemsWithMalformedIDs, dicItemsWithIDs, token);

                if (setDuplicateIDs.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDuplicatesNames))
                    {
                        foreach (IList<string> lstDuplicateNames in dicItemsWithIDs
                                                                    .Where(x => setDuplicateIDs.Contains(x.Key))
                                                                    .Select(x => x.Value))
                        {
                            token.ThrowIfCancellationRequested();
                            if (sbdDuplicatesNames.Length != 0)
                                sbdDuplicatesNames.AppendLine();
                            sbdDuplicatesNames.AppendJoin(Environment.NewLine, lstDuplicateNames);
                        }

                        Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo
                            , LanguageManager.GetString(
                                "Message_DuplicateGuidWarning", token: token)
                            , setDuplicateIDs.Count
                            , strFileName
                            , sbdDuplicatesNames.ToString()));
                    }
                }
            }

            if (lstItemsWithMalformedIDs.Count > 0)
            {
                Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo
                    , LanguageManager.GetString("Message_NonGuidIdWarning", token: token)
                    , lstItemsWithMalformedIDs.Count
                    , strFileName
                    , string.Join(Environment.NewLine, lstItemsWithMalformedIDs)));
            }
        }

        private static void CheckIdNode(XmlNode xmlParentNode, ICollection<string> setDuplicateIDs, ICollection<string> lstItemsWithMalformedIDs, IDictionary<string, IList<string>> dicItemsWithIDs, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (XmlNodeList xmlChildNodeList = xmlParentNode.SelectNodes("*"))
            {
                if (!(xmlChildNodeList?.Count > 0))
                    return;

                foreach (XmlNode xmlLoopNode in xmlChildNodeList)
                {
                    token.ThrowIfCancellationRequested();
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
                    CheckIdNode(xmlLoopNode, setDuplicateIDs, lstItemsWithMalformedIDs, dicItemsWithIDs, token);
                }
            }
        }

        private static void AppendTranslations(XmlDocument xmlDataDocument, XPathNavigator xmlTranslationListParentNode, XmlNode xmlDataParentNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (XPathNavigator objChild in xmlTranslationListParentNode.SelectAndCacheExpression("*"))
            {
                token.ThrowIfCancellationRequested();
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
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem, token);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("metavariants");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem, token);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("choices");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem, token);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("talents");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem, token);
                    }
                    xmlSubItemsNode = objChild.SelectSingleNodeAndCacheExpression("versions");
                    if (xmlSubItemsNode != null)
                    {
                        AppendTranslations(xmlDataDocument, xmlSubItemsNode, xmlItem, token);
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
        /// Fetch the first file from a list of custom file paths.
        /// </summary>
        /// <param name="strFileName">Name of the file to fetch.</param>
        /// <param name="lstPaths">Paths to check.</param>
        /// <param name="token">CancellationToken to use.</param>
        /// <returns>The first full path containing <paramref name="strFileName"/> if one is found, string.Empty otherwise.</returns>
        private static string FetchBaseFileFromCustomDataPaths(string strFileName, IEnumerable<string> lstPaths, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strFileName != "improvements.xml" && lstPaths != null)
            {
                foreach (string strLoopPath in lstPaths)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (string strLoop in Directory.EnumerateFiles(strLoopPath, strFileName, SearchOption.AllDirectories))
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                            return strLoop;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Filter through a list of data paths and return a list of ones that modify the given file. Used to recycle data files between different rule sets.
        /// </summary>
        /// <param name="strFileName">Name of the file that would be modified by custom data files.</param>
        /// <param name="lstPaths">Paths to check for custom data files relevant to <paramref name="strFileName"/>.</param>
        /// <param name="token">CancellationToken to use.</param>
        /// <returns>A list of paths with <paramref name="lstPaths"/> that is relevant to <paramref name="strFileName"/>, in the same order that they are in <paramref name="lstPaths"/>.</returns>
        private static IEnumerable<string> CompileRelevantCustomDataPaths(string strFileName, IEnumerable<string> lstPaths, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strFileName == "improvements.xml" || lstPaths == null)
                yield break;
            foreach (string strLoopPath in lstPaths)
            {
                token.ThrowIfCancellationRequested();
                if (!Directory.Exists(strLoopPath))
                    continue;
                foreach (string strLoopFile in Directory.EnumerateFiles(strLoopPath, "*_" + strFileName,
                                                                        SearchOption.AllDirectories))
                {
                    token.ThrowIfCancellationRequested();
                    string strInnerFileName = Path.GetFileName(strLoopFile);
                    if (strInnerFileName.StartsWith("override_") || strInnerFileName.StartsWith("custom_")
                                                                 || strInnerFileName.StartsWith("amend_"))
                    {
                        yield return strLoopPath;
                        break;
                    }
                }
            }
        }

        private static bool DoProcessCustomDataFiles(XmlDocument xmlFile, XmlDocument xmlDataDoc, string strLoopPath, string strFileName, SearchOption eSearchOption = SearchOption.AllDirectories, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnReturn = false;
            XmlElement objDocElement = xmlDataDoc.DocumentElement;
            List<string> lstPossibleCustomFiles = new List<string>(Utils.BasicDataFileNames.Count);
            foreach (string strFile in Directory.EnumerateFiles(strLoopPath, "*_" + strFileName, eSearchOption))
            {
                token.ThrowIfCancellationRequested();
                string strLoopFileName = Path.GetFileName(strFile);
                if (!strLoopFileName.StartsWith("override_")
                    && !strLoopFileName.StartsWith("custom_")
                    && !strLoopFileName.StartsWith("amend_"))
                    continue;
                lstPossibleCustomFiles.Add(strFile);
            }
            foreach (string strFile in lstPossibleCustomFiles)
            {
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            foreach (XmlNode objType in objNode.ChildNodes)
                            {
                                token.ThrowIfCancellationRequested();
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
                                                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0 && objDocElement != null)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            token.ThrowIfCancellationRequested();
                            // Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
                            List<XmlNode> lstDelete = new List<XmlNode>(objNode.ChildNodes.Count);
                            foreach (XmlNode objChild in objNode.ChildNodes)
                            {
                                token.ThrowIfCancellationRequested();
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
                                                token.ThrowIfCancellationRequested();
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
                                token.ThrowIfCancellationRequested();
                                objNode.RemoveChild(objRemoveNode);
                            }

                            XmlNode xmlExistingNode = objDocElement[objNode.Name];
                            if (xmlExistingNode != null
                                && xmlExistingNode.Attributes?.Count == objNode.Attributes?.Count)
                            {
                                bool blnAllMatching = true;
                                foreach (XmlAttribute x in xmlExistingNode.Attributes)
                                {
                                    token.ThrowIfCancellationRequested();
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
                                        token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();

                using (XmlNodeList xmlNodeList = xmlFile.SelectNodes("/chummer/*"))
                {
                    if (xmlNodeList?.Count > 0)
                    {
                        foreach (XmlNode objNode in xmlNodeList)
                        {
                            token.ThrowIfCancellationRequested();
                            blnReturn = AmendNodeChildren(xmlDataDoc, objNode, "/chummer", token: token) || blnReturn;
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
        /// <param name="token">CancellationToken to use.</param>
        /// <returns>True if any amends were made, False otherwise.</returns>
        private static bool AmendNodeChildren(XmlDocument xmlDoc, XmlNode xmlAmendingNode, string strXPath, IList<Tuple<XmlNode, string>> lstExtraNodesToAddIfNotFound = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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
                        token.ThrowIfCancellationRequested();

                        // Child Nodes marked with "isidnode" serve as additional identifier nodes, in case something needs modifying that uses neither a name nor an ID.
                        using (XmlNodeList xmlChildrenWithIds
                               = xmlAmendingNode.SelectNodes("child::*[@isidnode = " + bool.TrueString.CleanXPath() + ']'))
                        {
                            if (xmlChildrenWithIds != null)
                            {
                                foreach (XmlNode objExtraId in xmlChildrenWithIds)
                                {
                                    token.ThrowIfCancellationRequested();
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

                token.ThrowIfCancellationRequested();

                // Get info on whether this node should be appended if no target node is found
                XmlNode objAddIfNotFound = objAmendingNodeAttribs.RemoveNamedItem("addifnotfound");
                if (objAddIfNotFound != null)
                {
                    blnAddIfNotFoundAttributePresent = true;
                    blnAddIfNotFound = objAddIfNotFound.InnerText == bool.TrueString;
                }

                token.ThrowIfCancellationRequested();

                // Gets the RegEx pattern for if the node is meant to be a RegEx replace operation
                XmlNode objRegExPattern = objAmendingNodeAttribs.RemoveNamedItem("regexpattern");
                if (objRegExPattern != null)
                {
                    strRegexPattern = objRegExPattern.InnerText;
                }
            }

            token.ThrowIfCancellationRequested();

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
                            token.ThrowIfCancellationRequested();
                            XmlNode xmlLoop = xmlDoc.ImportNode(xmlAmendingNode, true);
                            token.ThrowIfCancellationRequested();
                            xmlParentNode.AppendChild(xmlLoop);
                        }

                        blnReturn = true;
                    }
                }

                return blnReturn;
            }

            string strNewXPath = strXPath + '/' + xmlAmendingNode.Name + strFilter;

            token.ThrowIfCancellationRequested();

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
                            token.ThrowIfCancellationRequested();
                            if (objChild.NodeType == XmlNodeType.Element)
                            {
                                lstElementChildren.Add(objChild);
                            }
                        }
                    }
                }

                token.ThrowIfCancellationRequested();

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
                            Program.ShowMessageBox(ex.ToString());
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

                token.ThrowIfCancellationRequested();

                // We found nodes to target with the amend!
                if (objNodesToEdit?.Count > 0 || (strOperation == "recurse" && !blnAddIfNotFound))
                {
                    // Recurse is special in that it doesn't directly target nodes, but does so indirectly through strNewXPath...
                    if (strOperation == "recurse")
                    {
                        if (lstElementChildren?.Count > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (!(lstExtraNodesToAddIfNotFound?.Count > 0) && objNodesToEdit?.Count > 0)
                            {
                                foreach (XmlNode objChild in lstElementChildren)
                                {
                                    blnReturn = AmendNodeChildren(xmlDoc, objChild, strNewXPath, token: token);
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
                                        lstExtraNodesToAddIfNotFound, token);
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
                            token.ThrowIfCancellationRequested();
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
                                                token.ThrowIfCancellationRequested();
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
                                                                    token.ThrowIfCancellationRequested();
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

                                                StripAmendAttributesRecursively(xmlChild, token);
                                                token.ThrowIfCancellationRequested();
                                                XmlNode xmlLoop = xmlDoc.ImportNode(xmlChild, true);
                                                token.ThrowIfCancellationRequested();
                                                objNodeToEdit.AppendChild(xmlLoop);
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
                                                        token.ThrowIfCancellationRequested();
                                                        StripAmendAttributesRecursively(xmlAmendingNode, token);
                                                        token.ThrowIfCancellationRequested();
                                                        XmlNode xmlLoop = xmlDoc.ImportNode(xmlAmendingNode, true);
                                                        token.ThrowIfCancellationRequested();
                                                        xmlGrandparentNode.AppendChild(xmlLoop);
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "replace":
                                        StripAmendAttributesRecursively(xmlAmendingNode, token);
                                        if (xmlParentNode != null)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            XmlNode xmlLoop = xmlDoc.ImportNode(xmlAmendingNode, true);
                                            token.ThrowIfCancellationRequested();
                                            xmlParentNode.ReplaceChild(xmlLoop, objNodeToEdit);
                                        }

                                        break;
                                    case "regexreplace":
                                        if (xmlAmendingNode.HasChildNodes)
                                        {
                                            foreach (XmlNode xmlChild in xmlAmendingNode.ChildNodes)
                                            {
                                                token.ThrowIfCancellationRequested();
                                                XmlNodeType eChildNodeType = xmlChild.NodeType;

                                                // Text, Attributes, and CDATA are subject to the RegexReplace
                                                if ((eChildNodeType == XmlNodeType.Text ||
                                                     eChildNodeType == XmlNodeType.Attribute ||
                                                     eChildNodeType == XmlNodeType.CDATA) && objNodeToEdit.HasChildNodes)
                                                {
                                                    foreach (XmlNode objChildToEdit in objNodeToEdit.ChildNodes)
                                                    {
                                                        token.ThrowIfCancellationRequested();
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
                                                                Program.ShowMessageBox(ex.ToString());
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
                                                token.ThrowIfCancellationRequested();
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
                                                        Program.ShowMessageBox(ex.ToString());
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
                                    StripAmendAttributesRecursively(xmlAmendingNode, token);
                                    foreach (XmlNode xmlParentNode in xmlParentNodeList)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        // Make sure we can't actually find any targets
                                        if (xmlParentNode.SelectSingleNode(xmlAmendingNode.Name + strFilter) == null)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            XmlNode xmlLoop = xmlDoc.ImportNode(xmlAmendingNode, true);
                                            token.ThrowIfCancellationRequested();
                                            xmlParentNode.AppendChild(xmlLoop);
                                        }
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
                            token.ThrowIfCancellationRequested();
                            using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPathToAdd))
                            {
                                if (!(xmlParentNodeList?.Count > 0))
                                    continue;
                                foreach (XmlNode xmlParentNode in xmlParentNodeList)
                                {
                                    token.ThrowIfCancellationRequested();
                                    XmlNode xmlLoop = xmlDoc.ImportNode(xmlNodeToAdd, false);
                                    token.ThrowIfCancellationRequested();
                                    xmlParentNode.AppendChild(xmlLoop);
                                }
                            }
                        }
                        token.ThrowIfCancellationRequested();

                        lstExtraNodesToAddIfNotFound
                            .Clear(); // Everything in the list up to this point has been added, so now we clear the list
                    }

                    using (XmlNodeList xmlParentNodeList = xmlDoc.SelectNodes(strXPath))
                    {
                        if (xmlParentNodeList?.Count > 0)
                        {
                            StripAmendAttributesRecursively(xmlAmendingNode, token);
                            foreach (XmlNode xmlParentNode in xmlParentNodeList)
                            {
                                token.ThrowIfCancellationRequested();
                                XmlNode xmlLoop = xmlDoc.ImportNode(xmlAmendingNode, true);
                                token.ThrowIfCancellationRequested();
                                xmlParentNode.AppendChild(xmlLoop);
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
        /// <param name="token">CancellationToken to use.</param>
        private static void StripAmendAttributesRecursively(XmlNode xmlNodeToStrip, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XmlAttributeCollection objAmendingNodeAttribs = xmlNodeToStrip.Attributes;
            if (objAmendingNodeAttribs?.Count > 0)
            {
                objAmendingNodeAttribs.RemoveNamedItem("isidnode");
                objAmendingNodeAttribs.RemoveNamedItem("xpathfilter");
                objAmendingNodeAttribs.RemoveNamedItem("amendoperation");
                objAmendingNodeAttribs.RemoveNamedItem("addifnotfound");
                objAmendingNodeAttribs.RemoveNamedItem("regexpattern");
            }
            token.ThrowIfCancellationRequested();

            if (xmlNodeToStrip.HasChildNodes)
                foreach (XmlNode xmlChildNode in xmlNodeToStrip.ChildNodes)
                    StripAmendAttributesRecursively(xmlChildNode, token);
        }

        public static bool AnyXslFiles(string strLanguage, IEnumerable<Character> lstCharacters = null, CancellationToken token = default)
        {
            GetXslFilesFromLocalDirectory(strLanguage, out bool blnReturn, out List<ListItem> _, lstCharacters, false, false, token);
            return blnReturn;
        }

        public static List<ListItem> GetXslFilesFromLocalDirectory(string strLanguage,
                                                                   IEnumerable<Character> lstCharacters = null, bool blnUsePool = false, CancellationToken token = default)
        {
            GetXslFilesFromLocalDirectory(strLanguage, out bool _, out List<ListItem> lstReturn, lstCharacters, true, blnUsePool, token);
            return lstReturn;
        }

        private static void GetXslFilesFromLocalDirectory(string strLanguage, out bool blnAnyItem, out List<ListItem> lstSheets, IEnumerable<Character> lstCharacters, bool blnDoList, bool blnUsePool, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            blnAnyItem = false;
            HashSet<string> setAddedSheetFileNames = blnDoList ? Utils.StringHashSetPool.Get() : null;
            try
            {
                token.ThrowIfCancellationRequested();
                if (lstCharacters != null)
                {
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(10);
                    else
                        lstSheets = null;
                    // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
                    foreach (Character objCharacter in lstCharacters)
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (XPathNavigator xmlSheet in objCharacter.LoadDataXPath("sheets.xml", strLanguage, token: token)
                                                                        .SelectAndCacheExpression(
                                                                            "/chummer/sheets[@lang="
                                                                            + strLanguage.CleanXPath()
                                                                            + "]/sheet[not(hide)]"))
                        {
                            token.ThrowIfCancellationRequested();
                            string strSheetFileName = xmlSheet.SelectSingleNodeAndCacheExpression("filename")?.Value;
                            if (string.IsNullOrEmpty(strSheetFileName))
                                continue;
                            if (!blnDoList)
                            {
                                blnAnyItem = true;
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            if (!setAddedSheetFileNames.Add(strSheetFileName))
                                continue;
                            token.ThrowIfCancellationRequested();
                            blnAnyItem = true;
                            lstSheets.Add(new ListItem(
                                              !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                                  StringComparison.OrdinalIgnoreCase)
                                                  ? Path.Combine(strLanguage, strSheetFileName)
                                                  : strSheetFileName,
                                              xmlSheet.SelectSingleNodeAndCacheExpression("name")?.Value
                                              ?? LanguageManager.GetString("String_Unknown", token: token)));
                        }
                    }
                }
                else
                {
                    XPathNodeIterator xmlIterator = LoadXPath("sheets.xml", null, strLanguage, token: token)
                        .SelectAndCacheExpression(
                            "/chummer/sheets[@lang=" + strLanguage.CleanXPath() + "]/sheet[not(hide)]");
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(xmlIterator.Count);
                    else
                        lstSheets = null;

                    foreach (XPathNavigator xmlSheet in xmlIterator)
                    {
                        token.ThrowIfCancellationRequested();
                        string strSheetFileName = xmlSheet.SelectSingleNodeAndCacheExpression("filename")?.Value;
                        if (string.IsNullOrEmpty(strSheetFileName))
                            continue;
                        if (!blnDoList)
                        {
                            blnAnyItem = true;
                            return;
                        }
                        token.ThrowIfCancellationRequested();
                        if (!setAddedSheetFileNames.Add(strSheetFileName))
                            continue;
                        token.ThrowIfCancellationRequested();
                        blnAnyItem = true;
                        lstSheets.Add(new ListItem(
                                          !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                              StringComparison.OrdinalIgnoreCase)
                                              ? Path.Combine(strLanguage, strSheetFileName)
                                              : strSheetFileName,
                                          xmlSheet.SelectSingleNodeAndCacheExpression("name")?.Value
                                          ?? LanguageManager.GetString("String_Unknown", token: token)));
                    }
                }
            }
            finally
            {
                if (setAddedSheetFileNames != null)
                    Utils.StringHashSetPool.Return(setAddedSheetFileNames);
            }
        }

        public static async Task<bool> AnyXslFilesAsync(string strLanguage, IEnumerable<Character> lstCharacters = null, CancellationToken token = default)
        {
            return (await GetXslFilesFromLocalDirectoryAsync(strLanguage, lstCharacters, false, false, token).ConfigureAwait(false)).Item1;
        }

        public static async Task<List<ListItem>> GetXslFilesFromLocalDirectoryAsync(string strLanguage,
                                                                   IEnumerable<Character> lstCharacters = null, bool blnUsePool = false, CancellationToken token = default)
        {
            return (await GetXslFilesFromLocalDirectoryAsync(strLanguage, lstCharacters, true, blnUsePool, token).ConfigureAwait(false)).Item2;
        }

        public static async Task<Tuple<bool, List<ListItem>>> GetXslFilesFromLocalDirectoryAsync(string strLanguage, IEnumerable<Character> lstCharacters, bool blnDoList, bool blnUsePool, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<ListItem> lstSheets = null;
            HashSet<string> setAddedSheetFileNames = blnDoList ? Utils.StringHashSetPool.Get() : null;
            try
            {
                token.ThrowIfCancellationRequested();
                if (lstCharacters != null)
                {
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(10);
                    // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
                    foreach (Character objCharacter in lstCharacters)
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (XPathNavigator xmlSheet in await (await objCharacter.LoadDataXPathAsync("sheets.xml", strLanguage, token: token).ConfigureAwait(false))
                                     .SelectAndCacheExpressionAsync(
                                         "/chummer/sheets[@lang="
                                         + strLanguage.CleanXPath()
                                         + "]/sheet[not(hide)]", token: token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            string strSheetFileName = (await xmlSheet.SelectSingleNodeAndCacheExpressionAsync("filename", token: token).ConfigureAwait(false))?.Value;
                            if (string.IsNullOrEmpty(strSheetFileName))
                                continue;
                            if (!blnDoList)
                                return new Tuple<bool, List<ListItem>>(true, null);
                            token.ThrowIfCancellationRequested();
                            if (!setAddedSheetFileNames.Add(strSheetFileName))
                                continue;
                            token.ThrowIfCancellationRequested();
                            lstSheets.Add(new ListItem(
                                              !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                                  StringComparison.OrdinalIgnoreCase)
                                                  ? Path.Combine(strLanguage, strSheetFileName)
                                                  : strSheetFileName,
                                              (await xmlSheet.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                              ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                        }
                    }
                }
                else
                {
                    XPathNodeIterator xmlIterator = await (await LoadXPathAsync("sheets.xml", null, strLanguage, token: token).ConfigureAwait(false))
                        .SelectAndCacheExpressionAsync(
                            "/chummer/sheets[@lang=" + strLanguage.CleanXPath() + "]/sheet[not(hide)]", token: token).ConfigureAwait(false);
                    if (blnDoList)
                        lstSheets = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(xmlIterator.Count);

                    foreach (XPathNavigator xmlSheet in xmlIterator)
                    {
                        token.ThrowIfCancellationRequested();
                        string strSheetFileName = (await xmlSheet.SelectSingleNodeAndCacheExpressionAsync("filename", token: token).ConfigureAwait(false))?.Value;
                        if (string.IsNullOrEmpty(strSheetFileName))
                            continue;
                        if (!blnDoList)
                            return new Tuple<bool, List<ListItem>>(true, null);
                        token.ThrowIfCancellationRequested();
                        if (!setAddedSheetFileNames.Add(strSheetFileName))
                            continue;
                        token.ThrowIfCancellationRequested();
                        lstSheets.Add(new ListItem(
                                          !strLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                              StringComparison.OrdinalIgnoreCase)
                                              ? Path.Combine(strLanguage, strSheetFileName)
                                              : strSheetFileName,
                                          (await xmlSheet.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                          ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false)));
                    }
                }
            }
            finally
            {
                if (setAddedSheetFileNames != null)
                    Utils.StringHashSetPool.Return(setAddedSheetFileNames);
            }
            return new Tuple<bool, List<ListItem>>(lstSheets != null && lstSheets.Count > 0, lstSheets);
        }

        /// <summary>
        /// Verify the contents of the language data translation file.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        /// <param name="lstBooks">List of books.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask Verify(string strLanguage, ICollection<string> lstBooks,
                                             CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return;
            XPathDocument objLanguageDoc;
            string strFilePath = Path.Combine(Utils.GetLanguageFolderPath, strLanguage + "_data.xml");

            try
            {
                using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                    objLanguageDoc = new XPathDocument(objXmlReader);
            }
            catch (IOException ex)
            {
                Program.ShowMessageBox(ex.ToString());
                return;
            }
            catch (XmlException ex)
            {
                Program.ShowMessageBox(ex.ToString());
                return;
            }

            token.ThrowIfCancellationRequested();

            XPathNavigator objLanguageNavigator = objLanguageDoc.CreateNavigator();

            string strLangPath = Path.Combine(Utils.GetLanguageFolderPath, "results_" + strLanguage + ".xml");
            using (FileStream objStream
                   = new FileStream(strLangPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                token.ThrowIfCancellationRequested();
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);
                    // <results>
                    await objWriter.WriteStartElementAsync("results", token: token).ConfigureAwait(false);

                    foreach (string strFile in Directory.EnumerateFiles(Utils.GetDataFolderPath, "*.xml"))
                    {
                        token.ThrowIfCancellationRequested();
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
                        XPathNavigator objLanguageRoot
                            = objLanguageNavigator.SelectSingleNode(
                                "/chummer/chummer[@file = " + strFileName.CleanXPath() + ']');
                        if (objLanguageRoot != null)
                            blnExists = true;

                        // <file name="x" needstobeadded="y">
                        await objWriter.WriteStartElementAsync("file", token: token).ConfigureAwait(false);
                        await objWriter.WriteAttributeStringAsync("name", strFileName, token: token).ConfigureAwait(false);

                        if (blnExists)
                        {
                            // Load the current English file.
                            XPathNavigator objEnglishDoc = await LoadXPathAsync(strFileName, token: token).ConfigureAwait(false);
                            XPathNavigator objEnglishRoot
                                = await objEnglishDoc.SelectSingleNodeAndCacheExpressionAsync("/chummer", token: token).ConfigureAwait(false);

                            foreach (XPathNavigator objType in objEnglishRoot.SelectChildren(XPathNodeType.Element))
                            {
                                token.ThrowIfCancellationRequested();
                                string strTypeName = objType.Name;
                                bool blnTypeWritten = false;
                                foreach (XPathNavigator objChild in objType.SelectChildren(XPathNodeType.Element))
                                {
                                    token.ThrowIfCancellationRequested();
                                    // If the Node has a source element, check it and see if it's in the list of books that were specified.
                                    // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                                    bool blnContinue = true;
                                    XPathNavigator xmlSource
                                        = await objChild.SelectSingleNodeAndCacheExpressionAsync("source", token: token).ConfigureAwait(false);
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
                                        if (strTypeName == "modifiers"
                                            && strFile.EndsWith("ranges.xml", StringComparison.OrdinalIgnoreCase))
                                            continue;

                                        string strChildName = objChild.Name;
                                        XPathNavigator xmlTranslatedType
                                            = objLanguageRoot.SelectSingleNode(strTypeName);
                                        XPathNavigator xmlName
                                            = await objChild.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false);
                                        // Look for a matching entry in the Language file.
                                        if (xmlName != null)
                                        {
                                            string strChildNameElement = xmlName.Value;
                                            XPathNavigator xmlNode
                                                = xmlTranslatedType?.SelectSingleNode(
                                                    strChildName + "[name = " + strChildNameElement.CleanXPath() + ']');
                                            if (xmlNode != null)
                                            {
                                                // A match was found, so see what elements, if any, are missing.
                                                bool blnTranslate = false;
                                                bool blnAltPage = false;
                                                bool blnAdvantage = false;
                                                bool blnDisadvantage = false;

                                                if (objChild.HasChildren)
                                                {
                                                    if (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                            "translate", token: token).ConfigureAwait(false) != null)
                                                        blnTranslate = true;

                                                    // Do not mark page as missing if the original does not have it.
                                                    if (await objChild.SelectSingleNodeAndCacheExpressionAsync("page", token: token).ConfigureAwait(false)
                                                        != null)
                                                    {
                                                        if (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                                "altpage", token: token).ConfigureAwait(false) != null)
                                                            blnAltPage = true;
                                                    }
                                                    else
                                                        blnAltPage = true;

                                                    if (strFile.EndsWith("mentors.xml",
                                                                         StringComparison.OrdinalIgnoreCase)
                                                        || strFile.EndsWith(
                                                            "paragons.xml", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                                "altadvantage", token: token).ConfigureAwait(false) != null)
                                                            blnAdvantage = true;
                                                        if (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                                "altdisadvantage", token: token).ConfigureAwait(false) != null)
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
                                                    if (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                            "@translate", token: token).ConfigureAwait(false) != null)
                                                        blnTranslate = true;
                                                }

                                                // At least one piece of data was missing so write out the result node.
                                                if (!blnTranslate || !blnAltPage || !blnAdvantage || !blnDisadvantage)
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                    }

                                                    // <results>
                                                    await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                    await objWriter.WriteElementStringAsync(
                                                        "name", strChildNameElement, token: token).ConfigureAwait(false);
                                                    if (!blnTranslate)
                                                        await objWriter.WriteElementStringAsync("missing", "translate", token: token).ConfigureAwait(false);
                                                    if (!blnAltPage)
                                                        await objWriter.WriteElementStringAsync("missing", "altpage", token: token).ConfigureAwait(false);
                                                    if (!blnAdvantage)
                                                        await objWriter.WriteElementStringAsync(
                                                            "missing", "altadvantage", token: token).ConfigureAwait(false);
                                                    if (!blnDisadvantage)
                                                        await objWriter.WriteElementStringAsync(
                                                            "missing", "altdisadvantage", token: token).ConfigureAwait(false);
                                                    // </results>
                                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                }
                                            }
                                            else
                                            {
                                                if (!blnTypeWritten)
                                                {
                                                    blnTypeWritten = true;
                                                    await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                }

                                                // No match was found, so write out that the data item is missing.
                                                // <result>
                                                await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                await objWriter.WriteAttributeStringAsync(
                                                    "needstobeadded", bool.TrueString, token: token).ConfigureAwait(false);
                                                await objWriter.WriteElementStringAsync("name", strChildNameElement, token: token).ConfigureAwait(false);
                                                // </result>
                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                            }

                                            if (strFileName == "metatypes.xml")
                                            {
                                                XPathNavigator xmlMetavariants
                                                    = await objChild.SelectSingleNodeAndCacheExpressionAsync(
                                                        "metavariants", token: token).ConfigureAwait(false);
                                                if (xmlMetavariants != null)
                                                {
                                                    foreach (XPathNavigator objMetavariant in await xmlMetavariants
                                                                 .SelectAndCacheExpressionAsync("metavariant", token: token).ConfigureAwait(false))
                                                    {
                                                        string strMetavariantName
                                                            = (await objMetavariant
                                                                .SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false)).Value;
                                                        XPathNavigator objTranslate =
                                                            objLanguageRoot.SelectSingleNode(
                                                                "metatypes/metatype[name = "
                                                                + strChildNameElement.CleanXPath()
                                                                + "]/metavariants/metavariant[name = "
                                                                + strMetavariantName.CleanXPath() + ']');
                                                        if (objTranslate != null)
                                                        {
                                                            bool blnTranslate
                                                                = await objTranslate
                                                                    .SelectSingleNodeAndCacheExpressionAsync(
                                                                        "translate", token: token).ConfigureAwait(false) != null;
                                                            bool blnAltPage
                                                                = await objTranslate
                                                                      .SelectSingleNodeAndCacheExpressionAsync(
                                                                          "altpage", token: token).ConfigureAwait(false)
                                                                  != null;

                                                            // Item exists, so make sure it has its translate attribute populated.
                                                            if (!blnTranslate || !blnAltPage)
                                                            {
                                                                if (!blnTypeWritten)
                                                                {
                                                                    blnTypeWritten = true;
                                                                    await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                                }

                                                                // <result>
                                                                await objWriter.WriteStartElementAsync("metavariants", token: token).ConfigureAwait(false);
                                                                await objWriter.WriteStartElementAsync("metavariant", token: token).ConfigureAwait(false);
                                                                await objWriter.WriteElementStringAsync(
                                                                    "name", strMetavariantName, token: token).ConfigureAwait(false);
                                                                if (!blnTranslate)
                                                                    await objWriter.WriteElementStringAsync(
                                                                        "missing", "translate", token: token).ConfigureAwait(false);
                                                                if (!blnAltPage)
                                                                    await objWriter.WriteElementStringAsync(
                                                                        "missing", "altpage", token: token).ConfigureAwait(false);
                                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                                // </result>
                                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!blnTypeWritten)
                                                            {
                                                                blnTypeWritten = true;
                                                                await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                            }

                                                            // <result>
                                                            await objWriter.WriteStartElementAsync("metavariants", token: token).ConfigureAwait(false);
                                                            await objWriter.WriteStartElementAsync("metavariant", token: token).ConfigureAwait(false);
                                                            await objWriter.WriteAttributeStringAsync(
                                                                "needstobeadded", bool.TrueString, token: token).ConfigureAwait(false);
                                                            await objWriter.WriteElementStringAsync(
                                                                "name", strMetavariantName, token: token).ConfigureAwait(false);
                                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                            // </result>
                                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
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
                                            XPathNavigator xmlText
                                                = await objChild.SelectSingleNodeAndCacheExpressionAsync("text", token: token).ConfigureAwait(false);
                                            // Look for a matching entry in the Language file.
                                            if (xmlText != null)
                                            {
                                                string strChildTextElement = xmlText.Value;
                                                XPathNavigator xmlNode
                                                    = xmlTranslatedType?.SelectSingleNode(
                                                        strChildName + "[text = " + strChildTextElement.CleanXPath()
                                                        + ']');
                                                if (xmlNode != null)
                                                {
                                                    // A match was found, so see what elements, if any, are missing.
                                                    bool blnTranslate
                                                        = await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                              "translate", token: token).ConfigureAwait(false) != null
                                                          || (await xmlNode.SelectSingleNodeAndCacheExpressionAsync(
                                                              "@translated", token: token).ConfigureAwait(false))?.Value == bool.TrueString;

                                                    // At least one piece of data was missing so write out the result node.
                                                    if (!blnTranslate)
                                                    {
                                                        if (!blnTypeWritten)
                                                        {
                                                            blnTypeWritten = true;
                                                            await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                        }

                                                        // <results>
                                                        await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                        await objWriter.WriteElementStringAsync(
                                                            "text", strChildTextElement, token: token).ConfigureAwait(false);
                                                        if (!blnTranslate)
                                                            await objWriter.WriteElementStringAsync(
                                                                "missing", "translate", token: token).ConfigureAwait(false);
                                                        // </results>
                                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                    }
                                                }
                                                else
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        await objWriter.WriteStartElementAsync(strTypeName, token: token);
                                                    }

                                                    // No match was found, so write out that the data item is missing.
                                                    // <result>
                                                    await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                    await objWriter.WriteAttributeStringAsync(
                                                        "needstobeadded", bool.TrueString, token: token).ConfigureAwait(false);
                                                    await objWriter.WriteElementStringAsync(
                                                        "text", strChildTextElement, token: token).ConfigureAwait(false);
                                                    // </result>
                                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string strChildInnerText = objChild.Value;
                                            if (!string.IsNullOrEmpty(strChildInnerText))
                                            {
                                                // The item does not have a name which means it should have a translate CharacterAttribute instead.
                                                XPathNavigator objNode
                                                    = xmlTranslatedType?.SelectSingleNode(
                                                        strChildName + "[. =" + strChildInnerText.CleanXPath() + ']');
                                                if (objNode != null)
                                                {
                                                    // Make sure the translate attribute is populated.
                                                    if (await objNode.SelectSingleNodeAndCacheExpressionAsync(
                                                            "@translate", token: token).ConfigureAwait(false) == null)
                                                    {
                                                        if (!blnTypeWritten)
                                                        {
                                                            blnTypeWritten = true;
                                                            await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                        }

                                                        // <result>
                                                        await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                        await objWriter.WriteElementStringAsync(
                                                            "name", strChildInnerText, token: token).ConfigureAwait(false);
                                                        await objWriter.WriteElementStringAsync("missing", "translate", token: token).ConfigureAwait(false);
                                                        // </result>
                                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                    }
                                                }
                                                else
                                                {
                                                    if (!blnTypeWritten)
                                                    {
                                                        blnTypeWritten = true;
                                                        await objWriter.WriteStartElementAsync(strTypeName, token: token).ConfigureAwait(false);
                                                    }

                                                    // No match was found, so write out that the data item is missing.
                                                    // <result>
                                                    await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                                    await objWriter.WriteAttributeStringAsync(
                                                        "needstobeadded", bool.TrueString, token: token).ConfigureAwait(false);
                                                    await objWriter.WriteElementStringAsync("name", strChildInnerText, token: token).ConfigureAwait(false);
                                                    // </result>
                                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (blnTypeWritten)
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }

                            // Now loop through the translation file and determine if there are any entries in there that are not part of the base content.
                            foreach (XPathNavigator objType in objLanguageRoot.SelectChildren(XPathNodeType.Element))
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (XPathNavigator objChild in objType.SelectChildren(XPathNodeType.Element))
                                {
                                    token.ThrowIfCancellationRequested();
                                    string strChildNameElement
                                        = (await objChild.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value;
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
                                            await objWriter.WriteStartElementAsync("noentry", token: token).ConfigureAwait(false);
                                            await objWriter.WriteStartElementAsync(strChildName, token: token).ConfigureAwait(false);
                                            await objWriter.WriteElementStringAsync("name", strChildNameElement, token: token).ConfigureAwait(false);
                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                            // </noentry>
                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                        else
                            await objWriter.WriteAttributeStringAsync("needstobeadded", bool.TrueString, token: token).ConfigureAwait(false);

                        // </file>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // </results>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}
