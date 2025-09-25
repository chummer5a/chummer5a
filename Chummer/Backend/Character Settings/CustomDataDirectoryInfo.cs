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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Chummer
{
    /// <summary>
    /// This class holds all information about an CustomDataDirectory
    /// </summary>
    public sealed class CustomDataDirectoryInfo : IComparable, IEquatable<CustomDataDirectoryInfo>,
        IComparable<CustomDataDirectoryInfo>, IHasInternalId
    {
        private ValueVersion _objMyVersion = new ValueVersion(1);
        private Guid _guid = Guid.NewGuid();

        public Exception XmlException { get; private set; }

        public bool HasManifest { get; private set; }

        private readonly List<DirectoryDependency> _lstDependencies = new List<DirectoryDependency>();
        private readonly List<DirectoryDependency> _lstIncompatibilities = new List<DirectoryDependency>();

        private readonly Dictionary<string, bool> _dicAuthorDictionary = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _dicDescriptionDictionary = new Dictionary<string, string>();

        public CustomDataDirectoryInfo(string strName, string strDirectoryPath)
        {
            Name = strName;
            DirectoryPath = strDirectoryPath;
            LoadConstructorData();
        }

        private CustomDataDirectoryInfo(string strName, string strDirectoryPath, bool blnDoConstructorData)
        {
            Name = strName;
            DirectoryPath = strDirectoryPath;
            if (blnDoConstructorData)
                LoadConstructorData();
        }

        public static async Task<CustomDataDirectoryInfo> CreateAsync(string strName, string strDirectoryPath,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CustomDataDirectoryInfo objReturn = new CustomDataDirectoryInfo(strName, strDirectoryPath, false);
            await objReturn.LoadConstructorDataAsync(token).ConfigureAwait(false);
            return objReturn;
        }

        #region Constructor Helper Methods
        private void LoadConstructorData(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Load all the needed data from xml and form the correct strings
            try
            {
                string strFullDirectory = Path.Combine(DirectoryPath, "manifest.xml");

                if (File.Exists(strFullDirectory))
                {
                    HasManifest = true;
                    XPathDocument xmlObjManifest = XPathDocumentExtensions.LoadStandardFromFilePatient(strFullDirectory, token: token);
                    XPathNavigator xmlNode = xmlObjManifest.CreateNavigator()
                                                           .SelectSingleNodeAndCacheExpression("manifest", token);

                    if (!xmlNode.TryGetField("version", ValueVersion.TryParse, out _objMyVersion))
                        _objMyVersion = new ValueVersion(1, 0);
                    xmlNode.TryGetGuidFieldQuickly("guid", ref _guid);

                    ConstructorGetManifestDescriptions(xmlNode, token);
                    ConstructorGetManifestAuthors(xmlNode, token);
                    ConstructorGetDependencies(xmlNode, token);
                    ConstructorGetIncompatibilities(xmlNode, token);
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                // Save the exception to show it later
                XmlException = ex;
                HasManifest = false;
            }
        }

        private async Task LoadConstructorDataAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Load all the needed data from xml and form the correct strings
            try
            {
                string strFullDirectory = Path.Combine(DirectoryPath, "manifest.xml");

                if (File.Exists(strFullDirectory))
                {
                    HasManifest = true;
                    XPathDocument xmlObjManifest = await XPathDocumentExtensions.LoadStandardFromFilePatientAsync(strFullDirectory, token: token).ConfigureAwait(false);
                    XPathNavigator xmlNode = xmlObjManifest.CreateNavigator()
                                                           .SelectSingleNodeAndCacheExpression("manifest", token);

                    if (!xmlNode.TryGetField("version", ValueVersion.TryParse, out _objMyVersion))
                        _objMyVersion = new ValueVersion(1, 0);
                    xmlNode.TryGetGuidFieldQuickly("guid", ref _guid);

                    ConstructorGetManifestDescriptions(xmlNode, token);
                    ConstructorGetManifestAuthors(xmlNode, token);
                    ConstructorGetDependencies(xmlNode, token);
                    ConstructorGetIncompatibilities(xmlNode, token);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                // Save the exception to show it later
                XmlException = ex;
                HasManifest = false;
            }
        }

        private void ConstructorGetManifestDescriptions(XPathNavigator xmlDocument, CancellationToken token = default)
        {
            foreach (XPathNavigator descriptionNode in xmlDocument.SelectAndCacheExpression(
                         "descriptions/description", token))
            {
                string language = string.Empty;
                descriptionNode.TryGetStringFieldQuickly("lang", ref language);

                if (!string.IsNullOrEmpty(language))
                {
                    string text = string.Empty;
                    if (descriptionNode.TryGetStringFieldQuickly("text", ref text))
                        _dicDescriptionDictionary.Add(language, text);
                }
            }
        }
        //Must be called after DisplayDictionary was populated!

        private void ConstructorGetManifestAuthors(XPathNavigator xmlDocument, CancellationToken token = default)
        {
            foreach (XPathNavigator objXmlNode in xmlDocument.SelectAndCacheExpression("authors/author", token))
            {
                string authorName = string.Empty;
                bool isMain = false;

                objXmlNode.TryGetStringFieldQuickly("name", ref authorName);
                objXmlNode.TryGetBoolFieldQuickly("main", ref isMain);

                if (!string.IsNullOrEmpty(authorName) && !_dicAuthorDictionary.ContainsKey(authorName))
                    //Maybe a stupid idea? But who would add two authors with the same name anyway?
                    _dicAuthorDictionary.Add(authorName, isMain);
            }
        }
        //Must be called after AuthorDictionary was populated!

        private void ConstructorGetDependencies(XPathNavigator xmlDocument, CancellationToken token = default)
        {
            foreach (XPathNavigator objXmlNode in xmlDocument.SelectAndCacheExpression("dependencies/dependency", token))
            {
                Guid guidId = Guid.Empty;
                string strDependencyName = string.Empty;

                objXmlNode.TryGetStringFieldQuickly("name", ref strDependencyName);
                objXmlNode.TryGetGuidFieldQuickly("guid", ref guidId);

                //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                if (string.IsNullOrEmpty(strDependencyName) || guidId == Guid.Empty)
                    continue;

                objXmlNode.TryGetField("maxversion", ValueVersion.TryParse, out ValueVersion objNewMaximumVersion);
                objXmlNode.TryGetField("minversion", ValueVersion.TryParse, out ValueVersion objNewMinimumVersion);

                DirectoryDependency objDependency
                    = new DirectoryDependency(strDependencyName, guidId, objNewMinimumVersion,
                                              objNewMaximumVersion);
                _lstDependencies.Add(objDependency);
            }
        }

        private void ConstructorGetIncompatibilities(XPathNavigator xmlDocument, CancellationToken token = default)
        {
            foreach (XPathNavigator objXmlNode in xmlDocument.SelectAndCacheExpression(
                         "incompatibilities/incompatibility", token))
            {
                Guid guidId = Guid.Empty;
                string strDependencyName = string.Empty;

                objXmlNode.TryGetStringFieldQuickly("name", ref strDependencyName);
                objXmlNode.TryGetGuidFieldQuickly("guid", ref guidId);

                //If there is no name any displays based on this are worthless and if there isn't a ID no comparisons will work
                if (string.IsNullOrEmpty(strDependencyName) || guidId == Guid.Empty)
                    continue;

                objXmlNode.TryGetField("maxversion", ValueVersion.TryParse, out ValueVersion objNewMaximumVersion);
                objXmlNode.TryGetField("minversion", ValueVersion.TryParse, out ValueVersion objNewMinimumVersion);

                DirectoryDependency objIncompatibility
                    = new DirectoryDependency(strDependencyName, guidId, objNewMinimumVersion,
                                              objNewMaximumVersion);
                _lstIncompatibilities.Add(objIncompatibility);
            }
        }

        #endregion Constructor Helper Methods

        /* This is unused right now, but maybe we need it later for some reason.
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
        }*/

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the dependent upon directory
        /// </summary>
        /// <param name="objCharacterSettings"></param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public string CheckDependency(CharacterSettings objCharacterSettings, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (objCharacterSettings.LockObject.EnterReadLock(token))
            {
                IReadOnlyList<CustomDataDirectoryInfo> lstEnabledCustomDataDirectoryInfos =
                    objCharacterSettings.EnabledCustomDataDirectoryInfos;
                IReadOnlyCollection<Guid> lstEnabledCustomDataDirectoryInfoGuids =
                    objCharacterSettings.EnabledCustomDataDirectoryInfoGuids;
                int intMyLoadOrderPosition
                    = lstEnabledCustomDataDirectoryInfos.FindIndex(x => x.Equals(this));
                List<CustomDataDirectoryInfo> lstEnabledCustomData
                    = new List<CustomDataDirectoryInfo>(DependenciesList.Count);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (DirectoryDependency dependency in DependenciesList)
                    {
                        token.ThrowIfCancellationRequested();
                        lstEnabledCustomData.Clear();
                        Guid objDependencyGuid = dependency.UniqueIdentifier;
                        if (lstEnabledCustomDataDirectoryInfoGuids.Contains(objDependencyGuid))
                            lstEnabledCustomData.AddRange(
                                lstEnabledCustomDataDirectoryInfos.Where(
                                    x => x.Guid.Equals(objDependencyGuid)));
                        if (lstEnabledCustomData.Count > 0)
                        {
                            // First check if we have any data whose version matches
                            ValueVersion objMinVersion = dependency.MinimumVersion;
                            ValueVersion objMaxVersion = dependency.MaximumVersion;
                            if (objMinVersion != default(ValueVersion) || objMaxVersion != default(ValueVersion))
                            {
                                bool blnMismatch;
                                if (objMinVersion != default(ValueVersion) && objMaxVersion != default(ValueVersion))
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(
                                        x => x.MyVersion < objMinVersion
                                             || x.MyVersion > objMaxVersion);
                                }
                                else if (objMinVersion != default(ValueVersion))
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(x => x.MyVersion < objMinVersion);
                                }
                                else
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(x => x.MyVersion > objMaxVersion);
                                }

                                if (blnMismatch)
                                {
                                    sbdReturn.AppendFormat(
                                            GlobalSettings.CultureInfo,
                                            LanguageManager.GetString("Tooltip_Dependency_VersionMismatch", token: token),
                                            lstEnabledCustomData[0].CurrentDisplayName, dependency.CurrentDisplayName)
                                        .AppendLine();
                                    continue;
                                }

                                // Remove all from the list where version does not match before moving on to load orders
                                if (objMinVersion != default(ValueVersion) && objMaxVersion != default(ValueVersion))
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion
                                                                        || x.MyVersion > objMaxVersion);
                                }
                                else if (objMinVersion != default(ValueVersion))
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion);
                                }
                                else
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion > objMaxVersion);
                                }
                            }

                            if (intMyLoadOrderPosition >= 0 && intMyLoadOrderPosition
                                < lstEnabledCustomDataDirectoryInfos.FindLastIndex(
                                    x => lstEnabledCustomData.Contains(x)))
                            {
                                sbdReturn.AppendFormat(GlobalSettings.CultureInfo, LanguageManager.GetString("Tooltip_Dependency_BadLoadOrder", token: token),
                                    lstEnabledCustomData[0].Name, Name).AppendLine();
                            }
                        }
                        else
                        {
                            //We don't even need to attempt to check any versions if all guids are mismatched
                            sbdReturn.AppendLine(dependency.CurrentDisplayName);
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the dependent upon directory
        /// </summary>
        /// <param name="objCharacterSettings"></param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the names of all missing dependencies as a single string</returns>
        public async Task<string> CheckDependencyAsync(CharacterSettings objCharacterSettings,
                                                            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await objCharacterSettings.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IReadOnlyList<CustomDataDirectoryInfo> lstEnabledCustomDataDirectoryInfos =
                    await objCharacterSettings.GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false);
                IReadOnlyCollection<Guid> lstEnabledCustomDataDirectoryInfoGuids =
                    await objCharacterSettings.GetEnabledCustomDataDirectoryInfoGuidsAsync(token).ConfigureAwait(false);
                int intMyLoadOrderPosition
                    = lstEnabledCustomDataDirectoryInfos.FindIndex(x => x.Equals(this));
                List<CustomDataDirectoryInfo> lstEnabledCustomData
                    = new List<CustomDataDirectoryInfo>(DependenciesList.Count);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    foreach (DirectoryDependency dependency in DependenciesList)
                    {
                        lstEnabledCustomData.Clear();
                        Guid objDependencyGuid = dependency.UniqueIdentifier;
                        if (lstEnabledCustomDataDirectoryInfoGuids.Contains(objDependencyGuid))
                            lstEnabledCustomData.AddRange(
                                lstEnabledCustomDataDirectoryInfos.Where(
                                    x => x.Guid.Equals(objDependencyGuid)));
                        if (lstEnabledCustomData.Count > 0)
                        {
                            // First check if we have any data whose version matches
                            ValueVersion objMinVersion = dependency.MinimumVersion;
                            ValueVersion objMaxVersion = dependency.MaximumVersion;
                            if (objMinVersion != default(ValueVersion) || objMaxVersion != default(ValueVersion))
                            {
                                bool blnMismatch;
                                if (objMinVersion != default(ValueVersion) && objMaxVersion != default(ValueVersion))
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(
                                        x => x.MyVersion < objMinVersion
                                             || x.MyVersion > objMaxVersion);
                                }
                                else if (objMinVersion != default(ValueVersion))
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(x => x.MyVersion < objMinVersion);
                                }
                                else
                                {
                                    blnMismatch = lstEnabledCustomData.TrueForAll(x => x.MyVersion > objMaxVersion);
                                }

                                if (blnMismatch)
                                {
                                    sbdReturn.AppendFormat(
                                            GlobalSettings.CultureInfo,
                                            await LanguageManager
                                                .GetStringAsync("Tooltip_Dependency_VersionMismatch", token: token)
                                                .ConfigureAwait(false),
                                            await lstEnabledCustomData[0].GetCurrentDisplayNameAsync(token)
                                                .ConfigureAwait(false),
                                            await dependency.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                        .AppendLine();
                                    continue;
                                }

                                // Remove all from the list where version does not match before moving on to load orders
                                if (objMinVersion != default(ValueVersion) && objMaxVersion != default(ValueVersion))
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion
                                                                        || x.MyVersion > objMaxVersion);
                                }
                                else if (objMinVersion != default(ValueVersion))
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion < objMinVersion);
                                }
                                else
                                {
                                    lstEnabledCustomData.RemoveAll(x => x.MyVersion > objMaxVersion);
                                }
                            }

                            if (intMyLoadOrderPosition >= 0 && intMyLoadOrderPosition
                                < lstEnabledCustomDataDirectoryInfos.FindLastIndex(
                                    x => lstEnabledCustomData.Contains(x)))
                            {
                                sbdReturn.AppendFormat(
                                    GlobalSettings.CultureInfo,
                                    await LanguageManager
                                        .GetStringAsync("Tooltip_Dependency_BadLoadOrder", token: token)
                                        .ConfigureAwait(false),
                                    lstEnabledCustomData[0].Name, Name).AppendLine();
                            }
                        }
                        else
                        {
                            //We don't even need to attempt to check any versions if all guids are mismatched
                            sbdReturn.AppendLine(await dependency.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the prohibited directories
        /// </summary>
        /// <param name="objCharacterSettings"></param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public string CheckIncompatibility(CharacterSettings objCharacterSettings, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<CustomDataDirectoryInfo> lstEnabledCustomData
                = new List<CustomDataDirectoryInfo>(IncompatibilitiesList.Count);
            using (objCharacterSettings.LockObject.EnterReadLock(token))
            {
                IReadOnlyList<CustomDataDirectoryInfo> lstEnabledCustomDataDirectoryInfos =
                    objCharacterSettings.EnabledCustomDataDirectoryInfos;
                IReadOnlyCollection<Guid> lstEnabledCustomDataDirectoryInfoGuids =
                    objCharacterSettings.EnabledCustomDataDirectoryInfoGuids;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (DirectoryDependency incompatibility in IncompatibilitiesList)
                    {
                        token.ThrowIfCancellationRequested();
                        Guid objIncompatibilityGuid = incompatibility.UniqueIdentifier;
                        //Use the fast HasSet.Contains to determine if any dependency is present
                        if (!lstEnabledCustomDataDirectoryInfoGuids.Contains(
                                objIncompatibilityGuid))
                            continue;
                        //We still need to filter out all the matching incompatibilities from objCharacterSettings.EnabledCustomDataDirectoryInfos to check their versions
                        lstEnabledCustomData.Clear();
                        if (lstEnabledCustomDataDirectoryInfoGuids.Contains(
                                objIncompatibilityGuid))
                            lstEnabledCustomData.AddRange(
                                lstEnabledCustomDataDirectoryInfos.Where(
                                    x => x.Guid.Equals(objIncompatibilityGuid)));
                        if (lstEnabledCustomData.Count == 0)
                            continue;
                        CustomDataDirectoryInfo objInfoToDisplay;
                        ValueVersion objMinVersion = incompatibility.MinimumVersion;
                        ValueVersion objMaxVersion = incompatibility.MaximumVersion;
                        if (objMinVersion != default(ValueVersion))
                        {
                            if (incompatibility.MaximumVersion != default(ValueVersion))
                                objInfoToDisplay = lstEnabledCustomData.Find(
                                    x => x.MyVersion >= objMinVersion || x.MyVersion <= objMaxVersion);
                            else
                                objInfoToDisplay
                                    = lstEnabledCustomData.Find(x => x.MyVersion >= objMinVersion);
                        }
                        else if (incompatibility.MaximumVersion != default(ValueVersion))
                        {
                            objInfoToDisplay
                                = lstEnabledCustomData.Find(x => x.MyVersion <= objMaxVersion);
                        }
                        else
                            objInfoToDisplay = lstEnabledCustomData[0];

                        //if the version is within the version range add it to the list.
                        if (objInfoToDisplay != default)
                        {
                            sbdReturn.AppendFormat(GlobalSettings.CultureInfo, LanguageManager.GetString("Tooltip_Incompatibility_VersionMismatch", token: token),
                                objInfoToDisplay.CurrentDisplayName, incompatibility.CurrentDisplayName).AppendLine();
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        /// <summary>
        /// Checks if any custom data is activated, that matches name and version of the prohibited directories
        /// </summary>
        /// <param name="objCharacterSettings"></param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the names of all prohibited custom data directories as a single string</returns>
        public async Task<string> CheckIncompatibilityAsync(CharacterSettings objCharacterSettings,
            CancellationToken token = default)
        {
            List<CustomDataDirectoryInfo> lstEnabledCustomData
                = new List<CustomDataDirectoryInfo>(IncompatibilitiesList.Count);
            IAsyncDisposable objLocker =
                await objCharacterSettings.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IReadOnlyList<CustomDataDirectoryInfo> lstEnabledCustomDataDirectoryInfos =
                    await objCharacterSettings.GetEnabledCustomDataDirectoryInfosAsync(token).ConfigureAwait(false);
                IReadOnlyCollection<Guid> lstEnabledCustomDataDirectoryInfoGuids =
                    await objCharacterSettings.GetEnabledCustomDataDirectoryInfoGuidsAsync(token).ConfigureAwait(false);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    foreach (DirectoryDependency incompatibility in IncompatibilitiesList)
                    {
                        Guid objIncompatibilityGuid = incompatibility.UniqueIdentifier;
                        //Use the fast HasSet.Contains to determine if any dependency is present
                        if (!lstEnabledCustomDataDirectoryInfoGuids.Contains(
                                objIncompatibilityGuid))
                            continue;
                        //We still need to filter out all the matching incompatibilities from objCharacterSettings.EnabledCustomDataDirectoryInfos to check their versions
                        lstEnabledCustomData.Clear();
                        if (lstEnabledCustomDataDirectoryInfoGuids.Contains(
                                objIncompatibilityGuid))
                            lstEnabledCustomData.AddRange(
                                lstEnabledCustomDataDirectoryInfos.Where(
                                    x => x.Guid.Equals(objIncompatibilityGuid)));
                        if (lstEnabledCustomData.Count == 0)
                            continue;
                        CustomDataDirectoryInfo objInfoToDisplay;
                        ValueVersion objMinVersion = incompatibility.MinimumVersion;
                        ValueVersion objMaxVersion = incompatibility.MaximumVersion;
                        if (objMinVersion != default(ValueVersion))
                        {
                            if (incompatibility.MaximumVersion != default(ValueVersion))
                                objInfoToDisplay = lstEnabledCustomData.Find(
                                    x => x.MyVersion >= objMinVersion || x.MyVersion <= objMaxVersion);
                            else
                                objInfoToDisplay
                                    = lstEnabledCustomData.Find(x => x.MyVersion >= objMinVersion);
                        }
                        else if (incompatibility.MaximumVersion != default(ValueVersion))
                        {
                            objInfoToDisplay
                                = lstEnabledCustomData.Find(x => x.MyVersion <= objMaxVersion);
                        }
                        else
                            objInfoToDisplay = lstEnabledCustomData[0];

                        //if the version is within the version range add it to the list.
                        if (objInfoToDisplay != default)
                        {
                            sbdReturn.AppendFormat(
                                GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync("Tooltip_Incompatibility_VersionMismatch", token: token)
                                    .ConfigureAwait(false),
                                await objInfoToDisplay.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                await incompatibility.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)).AppendLine();
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a string that displays which dependencies are missing or shouldn't be active to be displayed a tooltip.
        /// </summary>
        /// <param name="missingDependency">The string of all missing Dependencies</param>
        /// <param name="presentIncompatibilities">The string of all incompatibilities that are active</param>
        /// <returns></returns>
        public static string BuildIncompatibilityDependencyString(string missingDependency = "",
                                                                  string presentIncompatibilities = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strReturn = string.Empty;

            if (!string.IsNullOrEmpty(missingDependency))
            {
                strReturn = LanguageManager.GetString("Tooltip_Dependency_Missing", token: token) + Environment.NewLine
                    + missingDependency;
            }

            if (!string.IsNullOrEmpty(presentIncompatibilities))
            {
                if (!string.IsNullOrEmpty(strReturn))
                    strReturn += Environment.NewLine;
                strReturn += LanguageManager.GetString("Tooltip_Incompatibility_Present", token: token) + Environment.NewLine
                    + presentIncompatibilities;
            }

            return strReturn;
        }

        /// <summary>
        /// Creates a string that displays which dependencies are missing or shouldn't be active to be displayed a tooltip.
        /// </summary>
        /// <param name="missingDependency">The string of all missing Dependencies</param>
        /// <param name="presentIncompatibilities">The string of all incompatibilities that are active</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public static async Task<string> BuildIncompatibilityDependencyStringAsync(
            string missingDependency = "", string presentIncompatibilities = "", CancellationToken token = default)
        {
            string strReturn = string.Empty;

            if (!string.IsNullOrEmpty(missingDependency))
            {
                strReturn = await LanguageManager.GetStringAsync("Tooltip_Dependency_Missing", token: token)
                                                 .ConfigureAwait(false) + Environment.NewLine + missingDependency;
            }

            if (!string.IsNullOrEmpty(presentIncompatibilities))
            {
                if (!string.IsNullOrEmpty(strReturn))
                    strReturn += Environment.NewLine;
                strReturn += await LanguageManager.GetStringAsync("Tooltip_Incompatibility_Present", token: token)
                                                  .ConfigureAwait(false) + Environment.NewLine
                                                                         + presentIncompatibilities;
            }

            return strReturn;
        }

        #region Properties

        /// <summary>
        /// The name of the custom data directory
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The path to the Custom Data Directory
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// The version of the custom data directory
        /// </summary>
        public ValueVersion MyVersion => _objMyVersion;

        // /// <summary>
        // /// The Sha512 Hash of all non manifest.xml files in the directory
        // /// </summary>
        // public int Hash { get; private set; }

        /// <summary>
        /// A list of all dependencies each formatted as Tuple(guid, str name, int version, bool isMinimumVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> DependenciesList => _lstDependencies;

        /// <summary>
        /// A list of all incompatibilities each formatted as Tuple(int guid, str name, int version, bool ignoreVersion).
        /// </summary>
        public IReadOnlyList<DirectoryDependency> IncompatibilitiesList => _lstIncompatibilities;

        /// <summary>
        /// A Dictionary containing all Descriptions, which uses the language code as key
        /// </summary>
        public IReadOnlyDictionary<string, string> DescriptionDictionary => _dicDescriptionDictionary;

        private string _strDisplayDescriptionLanguage = GlobalSettings.Language;
        private string _strDisplayDescription;

        /// <summary>
        /// The description to display
        /// </summary>
        public string CurrentDisplayDescription
        {
            get
            {
                // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
                // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
                // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
                // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
                // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
                if (string.IsNullOrEmpty(_strDisplayDescription)
                    || _strDisplayDescriptionLanguage != GlobalSettings.Language)
                {
                    _strDisplayDescription = DisplayDescription(_strDisplayDescriptionLanguage = GlobalSettings.Language);
                }

                return _strDisplayDescription;
            }
        }

        public async Task<string> GetCurrentDisplayDescriptionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
            // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
            // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
            // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
            // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
            if (string.IsNullOrEmpty(_strDisplayDescription)
                    || _strDisplayDescriptionLanguage != GlobalSettings.Language)
            {
                return _strDisplayDescription = await DisplayDescriptionAsync(_strDisplayDescriptionLanguage = GlobalSettings.Language, token).ConfigureAwait(false);
            }
            return _strDisplayDescription;
        }

        public string DisplayDescription(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!File.Exists(Path.Combine(DirectoryPath, "manifest.xml")))
                return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestMissing", strLanguage, token: token);

            if (DescriptionDictionary.TryGetValue(strLanguage, out string description))
                return description.NormalizeLineEndings(true);

            if (!DescriptionDictionary.TryGetValue(GlobalSettings.DefaultLanguage, out description))
                return LanguageManager.GetString("Tooltip_CharacterOptions_ManifestDescriptionMissing", strLanguage, token: token);

            return LanguageManager.GetString("Tooltip_CharacterOptions_LanguageSpecificManifestMissing",
                                             strLanguage, token: token) +
                   Environment.NewLine + Environment.NewLine + description.NormalizeLineEndings(true);
        }

        public async Task<string> DisplayDescriptionAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!File.Exists(Path.Combine(DirectoryPath, "manifest.xml")))
                return await LanguageManager
                             .GetStringAsync("Tooltip_CharacterOptions_ManifestMissing", strLanguage, token: token)
                             .ConfigureAwait(false);

            if (DescriptionDictionary.TryGetValue(strLanguage, out string description))
                return description.NormalizeLineEndings(true);

            if (!DescriptionDictionary.TryGetValue(GlobalSettings.DefaultLanguage, out description))
                return await LanguageManager
                             .GetStringAsync("Tooltip_CharacterOptions_ManifestDescriptionMissing", strLanguage,
                                             token: token).ConfigureAwait(false);

            return await LanguageManager.GetStringAsync("Tooltip_CharacterOptions_LanguageSpecificManifestMissing",
                                                        strLanguage, token: token).ConfigureAwait(false) +
                   Environment.NewLine + Environment.NewLine + description.NormalizeLineEndings(true);
        }

        /// <summary>
        /// A Dictionary, that uses the author name as key and provides a bool if he is the main author
        /// </summary>
        public IReadOnlyDictionary<string, bool> AuthorDictionary => _dicAuthorDictionary;

        private CultureInfo _objDisplayAuthorsCulture = GlobalSettings.CultureInfo;
        private string _strDisplayAuthorsLanguage = GlobalSettings.Language;
        private string _strDisplayAuthors;

        /// <summary>
        /// A string containing all Authors formatted as Author(main), Author2
        /// </summary>
        public string CurrentDisplayAuthors
        {
            get
            {
                // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
                // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
                // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
                // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
                // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
                if (string.IsNullOrEmpty(_strDisplayAuthors) || !ReferenceEquals(_objDisplayAuthorsCulture, GlobalSettings.CultureInfo) || _strDisplayAuthorsLanguage != GlobalSettings.Language)
                {
                    _strDisplayAuthors = DisplayAuthors(_objDisplayAuthorsCulture = GlobalSettings.CultureInfo, _strDisplayAuthorsLanguage = GlobalSettings.Language);
                }

                return _strDisplayAuthors;
            }
        }

        public async Task<string> GetCurrentDisplayAuthorsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Custom version of lazy initialization (needed because it's not static), otherwise program crashes on startup
            // Explanation: LanguageManager.GetString seems to create some win32Window Objects and will cause Application.SetCompatibleTextRenderingDefault(false);
            // and Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException); to throw an exception if they are called after
            // SetProcessDPI(GlobalSettings.DpiScalingMethodSetting); in program.cs. To prevent any unexpected problems with moving those to methods to the start of
            // the global mutex LazyCreate() handles all the offending methods and should be called, when the CharacterSettings are opened.
            if (string.IsNullOrEmpty(_strDisplayAuthors) || !ReferenceEquals(_objDisplayAuthorsCulture, GlobalSettings.CultureInfo) || _strDisplayAuthorsLanguage != GlobalSettings.Language)
            {
                _strDisplayAuthors = await DisplayAuthorsAsync(_objDisplayAuthorsCulture = GlobalSettings.CultureInfo, _strDisplayAuthorsLanguage = GlobalSettings.Language, token).ConfigureAwait(false);
            }

            return _strDisplayAuthors;
        }

        public string DisplayAuthors(CultureInfo objCultureInfo, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDisplayAuthors))
            {
                token.ThrowIfCancellationRequested();
                foreach (KeyValuePair<string, bool> kvp in AuthorDictionary)
                {
                    token.ThrowIfCancellationRequested();
                    sbdDisplayAuthors.AppendLine(kvp.Value
                                                     ? string.Format(objCultureInfo, kvp.Key,
                                                                     LanguageManager.GetString(
                                                                         "String_IsMainAuthor", strLanguage, token: token))
                                                     : kvp.Key);
                }

                return sbdDisplayAuthors.ToTrimmedString();
            }
        }

        public async Task<string> DisplayAuthorsAsync(CultureInfo objCultureInfo, string strLanguage,
                                                         CancellationToken token = default)
        {
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdDisplayAuthors))
            {
                foreach (KeyValuePair<string, bool> kvp in AuthorDictionary)
                {
                    sbdDisplayAuthors.AppendLine(kvp.Value
                                                     ? string.Format(objCultureInfo, kvp.Key,
                                                                     await LanguageManager.GetStringAsync(
                                                                         "String_IsMainAuthor", strLanguage,
                                                                         token: token).ConfigureAwait(false))
                                                     : kvp.Key);
                }

                return sbdDisplayAuthors.ToTrimmedString();
            }
        }

        public Guid Guid => _guid;

        public void RandomizeGuid()
        {
            // We should not be randomizing Guids for infos that have a manifest with a set Guid
            if (HasManifest)
                throw new NotSupportedException();
            _guid = Guid.NewGuid();
        }

        public void CopyGuid(CustomDataDirectoryInfo other)
        {
            // We should not be copying Guids for infos that have a manifest with a set Guid
            if (HasManifest)
                throw new NotSupportedException();
            _guid = other._guid;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// The name including the Version in this format "NAME (Version)"
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return MyVersion == new ValueVersion(1)
                ? Name
                : string.Format(objCulture, "{0}{1}({2})", Name,
                                LanguageManager.GetString("String_Space", strLanguage, token: token), MyVersion);
        }

        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return MyVersion == new ValueVersion(1)
                ? Name
                : string.Format(objCulture, "{0}{1}({2})", Name,
                                await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                     .ConfigureAwait(false), MyVersion);
        }

        public string InternalId => Guid.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Key to use in character options files
        /// </summary>
        public string CharacterSettingsSaveKey => HasManifest ? InternalId + ">" + MyVersion.ToString() : Name;

        public static string GetIdFromCharacterSettingsSaveKey(string strKey)
        {
            int intSeparatorIndex = strKey.IndexOf('>');
            if (intSeparatorIndex >= 0 && intSeparatorIndex + 1 < strKey.Length)
            {
                string strReturn = strKey.Substring(0, intSeparatorIndex);
                if (strReturn.IsGuid())
                    return strReturn;
            }

            return string.Empty;
        }

        public static string GetIdFromCharacterSettingsSaveKey(string strKey, out ValueVersion objPreferredVersion)
        {
            int intSeparatorIndex = strKey.IndexOf('>');
            if (intSeparatorIndex >= 0 && intSeparatorIndex + 1 < strKey.Length)
            {
                string strReturn = strKey.Substring(0, intSeparatorIndex);
                if (strReturn.IsGuid())
                {
                    objPreferredVersion = new ValueVersion(strKey.Substring(intSeparatorIndex + 1));
                    return strReturn;
                }
            }

            objPreferredVersion = default;
            return string.Empty;
        }

        #endregion Properties

        #region Interface Implementations and Operators

        public int CompareTo(object obj)
        {
            if (obj is CustomDataDirectoryInfo objOtherDirectoryInfo)
                return CompareTo(objOtherDirectoryInfo);
            return 1;
        }

        public int CompareTo(CustomDataDirectoryInfo other)
        {
            int intReturn = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            if (intReturn == 0)
            {
                intReturn = string.Compare(DirectoryPath, other.DirectoryPath, StringComparison.OrdinalIgnoreCase);
                //Should basically never happen, because paths are supposed to be unique. But this "future proofs" it.
                if (intReturn == 0)
                    intReturn = MyVersion.CompareTo(other.MyVersion);
            }

            return intReturn;
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomDataDirectoryInfo objOther)
                return Equals(objOther);
            return false;
        }

        public bool Equals(CustomDataDirectoryInfo other)
        {
            //This should be enough to uniquely identify an object.
            return other != null && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                   DirectoryPath.Equals(other.DirectoryPath, StringComparison.OrdinalIgnoreCase) &&
                   (other.Guid == Guid || (!other.HasManifest && !HasManifest)) && other.MyVersion.Equals(MyVersion) &&
                   other.DependenciesList.CollectionEqual(DependenciesList) &&
                   other.IncompatibilitiesList.CollectionEqual(IncompatibilitiesList);
        }

        public override int GetHashCode()
        {
            int dependencyHash = DependenciesList.GetEnsembleHashCode();
            int incompatibilityHash = IncompatibilitiesList.GetEnsembleHashCode();
            Guid guid = HasManifest ? Guid : Guid.Empty;
            //Path and Guid should already be enough because they are both unique, but just to be sure.
            return (Name, guid, DirectoryPath, MyVersion, incompatibilityHash, dependencyHash).GetHashCode();
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

        #endregion Interface Implementations and Operators
    }

    /// <summary>
    /// Holds all information about an Dependency or Incompatibility
    /// </summary>
    public readonly struct DirectoryDependency : IEquatable<DirectoryDependency>
    {
        public DirectoryDependency(string name, Guid guid, ValueVersion minVersion, ValueVersion maxVersion)
        {
            Name = name;
            UniqueIdentifier = guid;
            MinimumVersion = minVersion;
            MaximumVersion = maxVersion;
        }

        public string Name { get; }

        public Guid UniqueIdentifier { get; }

        public ValueVersion MinimumVersion { get; }

        public ValueVersion MaximumVersion { get; }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string DisplayName(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage, token: token);

            if (MinimumVersion != default(ValueVersion))
            {
                return MaximumVersion != default(ValueVersion)
                    ? string.Format(objCulture, "{0}{1}({2}{1}-{1}{3})", Name, strSpace,
                                    MinimumVersion, MaximumVersion)
                    // If maxversion is not given, don't display decimal.max display > instead
                    : string.Format(objCulture, "{0}{1}({1}>{1}{2})", Name, strSpace,
                                    MinimumVersion);
            }

            return MaximumVersion != default(ValueVersion)
                // If minversion is not given, don't display decimal.min display < instead
                ? string.Format(objCulture, "{0}{1}({1}<{1}{2})", Name, strSpace, MaximumVersion)
                // If neither min and max version are given, just display the Name instead of the decimal.min and decimal.max
                : Name;
        }

        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);

            if (MinimumVersion != default(ValueVersion))
            {
                return MaximumVersion != default(ValueVersion)
                    ? string.Format(objCulture, "{0}{1}({2}{1}-{1}{3})", Name, strSpace, MinimumVersion,
                                    MaximumVersion)
                    // If maxversion is not given, don't display decimal.max display > instead
                    : string.Format(objCulture, "{0}{1}({1}>{1}{2})", Name, strSpace, MinimumVersion);
            }

            return MaximumVersion != default(ValueVersion)
                // If minversion is not given, don't display decimal.min display < instead
                ? string.Format(objCulture, "{0}{1}({1}<{1}{2})", Name, strSpace, MaximumVersion)
                // If neither min and max version are given, just display the Name instead of the decimal.min and decimal.max
                : Name;
        }

        /// <inheritdoc />
        public bool Equals(DirectoryDependency other)
        {
            return Name == other.Name && UniqueIdentifier.Equals(other.UniqueIdentifier)
                                      && Equals(MinimumVersion, other.MinimumVersion)
                                      && Equals(MaximumVersion, other.MaximumVersion);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is DirectoryDependency other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ UniqueIdentifier.GetHashCode();
                hashCode = (hashCode * 397) ^ MinimumVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ MaximumVersion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DirectoryDependency left, DirectoryDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DirectoryDependency left, DirectoryDependency right)
        {
            return !left.Equals(right);
        }
    }
}
