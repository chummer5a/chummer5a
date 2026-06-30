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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Downloads and applies custom data directory updates from GitHub releases and tracks update availability.
    /// </summary>
    public static class CustomDataDirectoryUpdater
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private const string StrGitHubUserAgent = "Chummer5a-CustomDataUpdater";
        private const string StrInstalledReleaseVersionsSubKey = "CustomDataInstalledReleaseVersions";

        private static readonly ConcurrentDictionary<string, ValueVersion> s_dicInstalledReleaseVersions
            = new ConcurrentDictionary<string, ValueVersion>(StringComparer.OrdinalIgnoreCase);

        private static bool s_blnInstalledReleaseVersionsLoaded;

        public sealed class RemoteReleaseInfo
        {
            /// <summary>
            /// Version parsed from the release tag name.
            /// </summary>
            public ValueVersion Version { get; set; }

            /// <summary>
            /// URL of a release asset or zipball to download.
            /// </summary>
            public string DownloadUrl { get; set; } = string.Empty;
        }

        /// <summary>
        /// Result of checking whether a custom data directory has a remote update available.
        /// </summary>
        public enum CustomDataUpdateCheckState
        {
            Unknown,
            NoUpdateLocation,
            UpToDate,
            UpdateAvailable,
            CheckFailed
        }

        /// <summary>
        /// Cached availability information for a custom data directory update check.
        /// </summary>
        public readonly struct CustomDataUpdateAvailability
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomDataUpdateAvailability"/> struct.
            /// </summary>
            /// <param name="eState">Outcome of the update availability check.</param>
            /// <param name="objRemoteVersion">Version reported by the remote release, if the check succeeded.</param>
            public CustomDataUpdateAvailability(CustomDataUpdateCheckState eState, ValueVersion objRemoteVersion)
            {
                State = eState;
                RemoteVersion = objRemoteVersion;
            }

            /// <summary>
            /// Outcome of the most recent update availability check.
            /// </summary>
            public CustomDataUpdateCheckState State { get; }

            /// <summary>
            /// Version reported by the remote release, if the check succeeded.
            /// </summary>
            public ValueVersion RemoteVersion { get; }

            /// <summary>
            /// Whether a newer remote version is available than the locally installed version.
            /// </summary>
            public bool IsUpdateAvailable => State == CustomDataUpdateCheckState.UpdateAvailable;
        }

        private static readonly ConcurrentDictionary<string, CustomDataUpdateAvailability> s_dicUpdateAvailabilityCache
            = new ConcurrentDictionary<string, CustomDataUpdateAvailability>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the cached update availability for a custom data directory, or Unknown if it has not been checked yet.
        /// </summary>
        /// <param name="objInfo">Custom data directory to look up.</param>
        /// <returns>The cached update availability for the directory.</returns>
        public static CustomDataUpdateAvailability GetCachedAvailability(CustomDataDirectoryInfo objInfo)
        {
            if (objInfo == null)
                return new CustomDataUpdateAvailability(CustomDataUpdateCheckState.Unknown, default);
            return s_dicUpdateAvailabilityCache.GetOrAdd(objInfo.InternalId,
                _ => new CustomDataUpdateAvailability(CustomDataUpdateCheckState.Unknown, default));
        }

        /// <summary>
        /// Whether any cached custom data directory has an update available.
        /// </summary>
        /// <returns>True if at least one cached directory has an update available.</returns>
        public static bool HasAnyUpdatesAvailable()
        {
            foreach (CustomDataUpdateAvailability objAvailability in s_dicUpdateAvailabilityCache.Values)
            {
                if (objAvailability.IsUpdateAvailable)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the cached update availability for a custom data directory.
        /// </summary>
        /// <param name="objInfo">Custom data directory whose cache entry should be removed.</param>
        public static void InvalidateAvailabilityCache(CustomDataDirectoryInfo objInfo)
        {
            if (objInfo != null)
                s_dicUpdateAvailabilityCache.TryRemove(objInfo.InternalId, out _);
        }

        /// <summary>
        /// Whether the given URL is a supported GitHub releases update location.
        /// </summary>
        /// <param name="strUpdateLocation">URL to validate.</param>
        /// <returns>True if the URL can be resolved to a GitHub latest-release API endpoint.</returns>
        public static bool IsSupportedUpdateLocation(string strUpdateLocation)
            => TryGetGitHubLatestReleaseApiUrl(strUpdateLocation, out _);

        /// <summary>
        /// Gets the local version used for update comparison, taking the higher of the manifest version and the last
        /// successfully installed release tag.
        /// </summary>
        /// <param name="objInfo">Custom data directory whose installed version should be evaluated.</param>
        /// <returns>The effective local version used when comparing against remote releases.</returns>
        public static ValueVersion GetEffectiveLocalVersion(CustomDataDirectoryInfo objInfo)
        {
            if (objInfo == null)
                return default;
            EnsureInstalledReleaseVersionsLoaded();
            ValueVersion objVersion = objInfo.MyVersion;
            if (s_dicInstalledReleaseVersions.TryGetValue(objInfo.InternalId, out ValueVersion objInstalledVersion)
                && objInstalledVersion > objVersion)
            {
                objVersion = objInstalledVersion;
            }

            return objVersion;
        }

        /// <summary>
        /// Records the release tag version that was successfully installed for a custom data directory.
        /// </summary>
        /// <param name="objInfo">Custom data directory that was updated.</param>
        /// <param name="objVersion">Release tag version that was installed.</param>
        public static void SetInstalledReleaseVersion(CustomDataDirectoryInfo objInfo, ValueVersion objVersion)
        {
            if (objInfo == null)
                return;
            EnsureInstalledReleaseVersionsLoaded();
            s_dicInstalledReleaseVersions[objInfo.InternalId] = objVersion;
            try
            {
                RegistryKey objKey = Registry.CurrentUser.CreateSubKey(
                    "Software\\Chummer5\\" + StrInstalledReleaseVersionsSubKey, true);
                objKey?.SetValue(objInfo.InternalId, objVersion.ToString(), RegistryValueKind.String);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Warn(ex, "Failed to save installed custom data release version for {0}", objInfo.Name);
            }
        }

        /// <summary>
        /// Writes or updates the updatelocation element in a custom data directory's manifest.xml.
        /// </summary>
        /// <param name="strDirectoryPath">Path to the custom data directory.</param>
        /// <param name="strUpdateLocation">GitHub releases URL to save.</param>
        /// <param name="strError">Error message if the operation fails.</param>
        /// <returns>True if the manifest was updated successfully.</returns>
        public static bool TrySetUpdateLocationInManifest(string strDirectoryPath, string strUpdateLocation,
            out string strError)
        {
            strError = string.Empty;
            string strManifestPath = Path.Combine(strDirectoryPath, "manifest.xml");
            if (!File.Exists(strManifestPath))
            {
                strError = "manifest.xml not found";
                return false;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(strManifestPath);
                XmlNode xmlManifest = xmlDocument.SelectSingleNode("//*[local-name()='manifest']")
                                      ?? xmlDocument.DocumentElement;
                if (xmlManifest == null)
                {
                    strError = "manifest element not found";
                    return false;
                }

                XmlNode xmlUpdateLocation = xmlManifest.SelectSingleNode("*[local-name()='updatelocation']");
                if (xmlUpdateLocation == null)
                {
                    xmlUpdateLocation = xmlDocument.CreateElement("updatelocation");
                    XmlNode xmlVersion = xmlManifest.SelectSingleNode("*[local-name()='version']");
                    if (xmlVersion?.NextSibling != null)
                        xmlManifest.InsertAfter(xmlUpdateLocation, xmlVersion);
                    else
                        xmlManifest.AppendChild(xmlUpdateLocation);
                }

                xmlUpdateLocation.InnerText = strUpdateLocation.Trim();
                xmlDocument.Save(strManifestPath);
                return true;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                strError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Checks a custom data directory for a remote update and caches the result.
        /// </summary>
        /// <param name="objInfo">Custom data directory to check.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of the update availability check.</returns>
        public static async Task<CustomDataUpdateAvailability> CheckUpdateAvailabilityAsync(
            CustomDataDirectoryInfo objInfo, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objInfo == null)
                return new CustomDataUpdateAvailability(CustomDataUpdateCheckState.Unknown, default);
            if (!objInfo.HasUpdateLocation)
            {
                CustomDataUpdateAvailability objNoLocation
                    = new CustomDataUpdateAvailability(CustomDataUpdateCheckState.NoUpdateLocation, default);
                s_dicUpdateAvailabilityCache[objInfo.InternalId] = objNoLocation;
                return objNoLocation;
            }

            try
            {
                RemoteReleaseInfo objRemoteRelease
                    = await GetRemoteReleaseInfoAsync(objInfo.UpdateLocation, token).ConfigureAwait(false);
                ValueVersion objLocalVersion = GetEffectiveLocalVersion(objInfo);
                CustomDataUpdateAvailability objResult = objRemoteRelease.Version > objLocalVersion
                    ? new CustomDataUpdateAvailability(CustomDataUpdateCheckState.UpdateAvailable,
                        objRemoteRelease.Version)
                    : new CustomDataUpdateAvailability(CustomDataUpdateCheckState.UpToDate, objRemoteRelease.Version);
                s_dicUpdateAvailabilityCache[objInfo.InternalId] = objResult;
                return objResult;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Warn(ex, "Failed to check custom data update availability for {0}", objInfo.Name);
                CustomDataUpdateAvailability objFailed
                    = new CustomDataUpdateAvailability(CustomDataUpdateCheckState.CheckFailed, default);
                s_dicUpdateAvailabilityCache[objInfo.InternalId] = objFailed;
                return objFailed;
            }
        }

        /// <summary>
        /// Reloads each custom data directory from disk and checks all directories that have an update location.
        /// </summary>
        /// <param name="lstInfos">Custom data directories to reload and check.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task CheckAllUpdateAvailabilitiesAsync(
            IEnumerable<CustomDataDirectoryInfo> lstInfos, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstInfos == null)
                return;
            foreach (CustomDataDirectoryInfo objInfo in lstInfos)
            {
                if (objInfo == null)
                    continue;
                CustomDataDirectoryInfo objFreshInfo;
                try
                {
                    objFreshInfo = await CustomDataDirectoryInfo
                                         .CreateAsync(objInfo.Name, objInfo.DirectoryPath, token)
                                         .ConfigureAwait(false);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    ex = ex.Demystify();
                    Log.Warn(ex, "Failed to reload custom data directory {0} for update check", objInfo.Name);
                    continue;
                }

                if (objFreshInfo.HasUpdateLocation)
                    await CheckUpdateAvailabilityAsync(objFreshInfo, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the display name for a custom data directory in a list, including an update-available suffix if applicable.
        /// </summary>
        /// <param name="objInfo">Custom data directory whose name should be displayed.</param>
        /// <param name="strLanguage">Language code to use for localized text, or null to use the current UI language.</param>
        /// <returns>The localized list display name.</returns>
        public static string GetListDisplayName(CustomDataDirectoryInfo objInfo, string strLanguage = null)
        {
            return AppendUpdateIndicatorToDisplayName(objInfo?.Name ?? string.Empty, objInfo, strLanguage);
        }

        /// <summary>
        /// Appends an update-available suffix to a display name when a newer remote version is cached.
        /// </summary>
        /// <param name="strDisplayName">Base display name to decorate.</param>
        /// <param name="objInfo">Custom data directory whose cached availability should be consulted.</param>
        /// <param name="strLanguage">Language code to use for localized text, or null to use the current UI language.</param>
        /// <returns>The display name, with an update-available suffix appended when applicable.</returns>
        public static string AppendUpdateIndicatorToDisplayName(string strDisplayName,
            CustomDataDirectoryInfo objInfo, string strLanguage = null)
        {
            if (objInfo == null || string.IsNullOrEmpty(strDisplayName))
                return strDisplayName ?? string.Empty;
            CustomDataUpdateAvailability objAvailability = GetCachedAvailability(objInfo);
            if (!objAvailability.IsUpdateAvailable)
                return strDisplayName;
            return string.Format(GlobalSettings.CultureInfo,
                LanguageManager.GetString("String_CustomData_UpdateAvailableInList",
                    strLanguage ?? GlobalSettings.Language),
                strDisplayName, objAvailability.RemoteVersion);
        }

        /// <summary>
        /// Gets localized version text for a custom data directory, including the remote version when an update is available.
        /// </summary>
        /// <param name="objInfo">Custom data directory whose version should be displayed.</param>
        /// <param name="strLanguage">Language code to use for localized text, or null to use the current UI language.</param>
        /// <returns>Localized version text for the directory.</returns>
        public static string GetVersionDisplayText(CustomDataDirectoryInfo objInfo, string strLanguage = null)
        {
            if (objInfo == null)
                return string.Empty;
            CustomDataUpdateAvailability objAvailability = GetCachedAvailability(objInfo);
            ValueVersion objLocalVersion = GetEffectiveLocalVersion(objInfo);
            if (!objAvailability.IsUpdateAvailable)
                return objLocalVersion.ToString();
            return string.Format(GlobalSettings.CultureInfo,
                LanguageManager.GetString("String_CustomData_RemoteVersionAvailable",
                    strLanguage ?? GlobalSettings.Language),
                objLocalVersion, objAvailability.RemoteVersion);
        }

        /// <summary>
        /// Fetches metadata for the latest GitHub release at the given update location.
        /// </summary>
        /// <param name="strUpdateLocation">GitHub releases URL to query.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Metadata for the latest release, including its version and download URL.</returns>
        /// <exception cref="NotSupportedException">The update location is not a supported GitHub releases URL.</exception>
        /// <exception cref="InvalidDataException">The release response did not contain a usable version or download URL.</exception>
        public static async Task<RemoteReleaseInfo> GetRemoteReleaseInfoAsync(string strUpdateLocation,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!TryGetGitHubLatestReleaseApiUrl(strUpdateLocation, out Uri uriApiUrl))
                throw new NotSupportedException();

            string strJson = await DownloadStringAsync(uriApiUrl, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            JObject objRelease = JObject.Parse(strJson);
            string strTagName = objRelease["tag_name"]?.ToString();
            if (string.IsNullOrEmpty(strTagName) || !ValueVersion.TryParse(strTagName.TrimStart('v', 'V'), out ValueVersion objVersion))
                throw new InvalidDataException();

            string strDownloadUrl = string.Empty;
            if (objRelease["assets"] is JArray objAssets)
            {
                foreach (JToken objAsset in objAssets)
                {
                    string strAssetUrl = objAsset["browser_download_url"]?.ToString();
                    if (!string.IsNullOrEmpty(strAssetUrl)
                        && strAssetUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        strDownloadUrl = strAssetUrl;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(strDownloadUrl))
                strDownloadUrl = objRelease["zipball_url"]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(strDownloadUrl))
                throw new InvalidDataException();

            return new RemoteReleaseInfo
            {
                Version = objVersion,
                DownloadUrl = strDownloadUrl
            };
        }

        /// <summary>
        /// Downloads and installs a remote release into a custom data directory after verifying its GUID.
        /// </summary>
        /// <param name="objInfo">Custom data directory to update.</param>
        /// <param name="objRemoteRelease">Release metadata returned by <see cref="GetRemoteReleaseInfoAsync"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>An empty string on success, or a localized or exception error message on failure. If the downloaded
        /// manifest includes an <c>updatelocation</c> element, that value is kept; otherwise any local
        /// <c>updatelocation</c> is restored after installation.</returns>
        public static async Task<string> ApplyUpdateAsync(CustomDataDirectoryInfo objInfo, RemoteReleaseInfo objRemoteRelease,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objInfo == null)
                throw new ArgumentNullException(nameof(objInfo));
            if (objRemoteRelease == null)
                throw new ArgumentNullException(nameof(objRemoteRelease));
            if (string.IsNullOrEmpty(objRemoteRelease.DownloadUrl))
                return await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false);

            string strLocalUpdateLocation = objInfo.UpdateLocation;
            string strTempRoot = Path.Combine(Path.GetTempPath(), "Chummer5a", "customdata-update",
                                              Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo));
            string strTempZipPath = Path.Combine(strTempRoot, "download.zip");
            string strTempExtractPath = Path.Combine(strTempRoot, "extract");
            try
            {
                Directory.CreateDirectory(strTempRoot);
                token.ThrowIfCancellationRequested();
                await DownloadFileAsync(new Uri(objRemoteRelease.DownloadUrl), strTempZipPath, token)
                    .ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                Directory.CreateDirectory(strTempExtractPath);
                if (!TryExtractCustomDataZip(strTempZipPath, strTempExtractPath, token, out string strExtractError))
                    return strExtractError;

                token.ThrowIfCancellationRequested();
                if (!TryFindCustomDataContentRoot(strTempExtractPath, objInfo.Guid, token,
                        out string strContentRoot, out string strManifestPath))
                {
                    return await LanguageManager
                               .GetStringAsync("Message_CustomDataDirectory_UpdateManifestMissing", token: token)
                               .ConfigureAwait(false);
                }

                if (!TryGetManifestGuid(strManifestPath, out Guid guidRemoteManifest, token)
                    || guidRemoteManifest != objInfo.Guid)
                {
                    return await LanguageManager
                               .GetStringAsync("Message_CustomDataDirectory_UpdateGuidMismatch", token: token)
                               .ConfigureAwait(false);
                }

                string strPreserveUpdateLocation = string.Empty;
                if (!TryGetManifestUpdateLocation(strManifestPath, out string strRemoteUpdateLocation, token)
                    || string.IsNullOrWhiteSpace(strRemoteUpdateLocation))
                {
                    strPreserveUpdateLocation = strLocalUpdateLocation;
                }

                token.ThrowIfCancellationRequested();
                ReplaceDirectoryContents(strContentRoot, objInfo.DirectoryPath, token);
                if (!string.IsNullOrEmpty(strPreserveUpdateLocation))
                    TrySetUpdateLocationInManifest(objInfo.DirectoryPath, strPreserveUpdateLocation, out _);
                SetInstalledReleaseVersion(objInfo, objRemoteRelease.Version);
                InvalidateAvailabilityCache(objInfo);
                return string.Empty;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Warn(ex, "Failed to update custom data directory {0}", objInfo.Name);
                return ex.Message;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(strTempRoot))
                        Directory.Delete(strTempRoot, true);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Log.Warn(ex, "Failed to clean up temporary custom data update folder {0}", strTempRoot);
                }
            }
        }

        private static void EnsureInstalledReleaseVersionsLoaded()
        {
            if (s_blnInstalledReleaseVersionsLoaded)
                return;
            s_blnInstalledReleaseVersionsLoaded = true;
            try
            {
                RegistryKey objKey = Registry.CurrentUser.OpenSubKey(
                    "Software\\Chummer5\\" + StrInstalledReleaseVersionsSubKey);
                if (objKey == null)
                    return;
                foreach (string strGuid in objKey.GetValueNames())
                {
                    if (ValueVersion.TryParse(objKey.GetValue(strGuid)?.ToString(), out ValueVersion objVersion))
                        s_dicInstalledReleaseVersions[strGuid] = objVersion;
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Warn(ex, "Failed to load installed custom data release versions from registry");
            }
        }

        private static bool TryGetGitHubLatestReleaseApiUrl(string strUpdateLocation, out Uri uriApiUrl)
        {
            uriApiUrl = null;
            if (!Uri.TryCreate(strUpdateLocation.Trim(), UriKind.Absolute, out Uri uriUpdateLocation))
                return false;

            if (uriUpdateLocation.Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase))
            {
                string strPath = uriUpdateLocation.AbsolutePath.TrimEnd('/');
                if (!strPath.EndsWith("/latest", StringComparison.OrdinalIgnoreCase)
                    && strPath.Contains("/releases", StringComparison.OrdinalIgnoreCase))
                {
                    uriApiUrl = new Uri(strPath + "/latest");
                }
                else
                {
                    uriApiUrl = uriUpdateLocation;
                }

                return true;
            }

            if (!uriUpdateLocation.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                return false;

            string[] astrPathParts = uriUpdateLocation.AbsolutePath.Trim('/').Split('/');
            if (astrPathParts.Length < 2)
                return false;

            uriApiUrl = new Uri(
                "https://api.github.com/repos/" + astrPathParts[0] + "/" + astrPathParts[1] + "/releases/latest");
            return true;
        }

        private static async Task<string> DownloadStringAsync(Uri uriAddress, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            HttpWebRequest objRequest = WebRequest.Create(uriAddress) as HttpWebRequest
                                        ?? throw new InvalidOperationException();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            objRequest.UserAgent = StrGitHubUserAgent;
            objRequest.Accept = "application/json";
            objRequest.Timeout = 15000;

            using (HttpWebResponse objResponse =
                   await objRequest.GetResponseAsync().ConfigureAwait(false) as HttpWebResponse)
            {
                token.ThrowIfCancellationRequested();
                using (Stream objStream = objResponse?.GetResponseStream())
                {
                    if (objStream == null)
                        throw new IOException();
                    using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                    {
                        return await objReader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task DownloadFileAsync(Uri uriAddress, string strDestinationPath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using (WebClient objClient = new WebClient())
            {
                objClient.Headers.Add("User-Agent", StrGitHubUserAgent);
                await objClient.DownloadFileTaskAsync(uriAddress, strDestinationPath).ConfigureAwait(false);
            }
        }

        private static bool TryExtractCustomDataZip(string strZipPath, string strDestinationPath,
            CancellationToken token, out string strError)
        {
            strError = string.Empty;
            try
            {
                using (ZipArchive objZipArchive
                       = ZipFile.Open(strZipPath, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                {
                    token.ThrowIfCancellationRequested();
                    string strRootPrefix = GetZipRootPrefix(objZipArchive);
                    foreach (ZipArchiveEntry objEntry in objZipArchive.Entries)
                    {
                        token.ThrowIfCancellationRequested();
                        string strFullName = objEntry.FullName;
                        if (strFullName.Length > 0 && strFullName[strFullName.Length - 1] == '/')
                            continue;
                        if (!string.IsNullOrEmpty(strRootPrefix)
                            && !strFullName.StartsWith(strRootPrefix, StringComparison.Ordinal))
                            continue;
                        string strRelativePath = string.IsNullOrEmpty(strRootPrefix)
                            ? strFullName
                            : strFullName.Substring(strRootPrefix.Length);
                        if (string.IsNullOrEmpty(strRelativePath)
                            || objEntry.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                            continue;

                        string strLoopPath = Path.GetFullPath(Path.Combine(strDestinationPath, strRelativePath));
                        if (!strLoopPath.StartsWith(Path.GetFullPath(strDestinationPath + Path.DirectorySeparatorChar),
                                                    StringComparison.OrdinalIgnoreCase))
                            continue;

                        string strLoopDirectory = Path.GetDirectoryName(strLoopPath);
                        if (!string.IsNullOrEmpty(strLoopDirectory))
                            Directory.CreateDirectory(strLoopDirectory);
                        objEntry.ExtractToFile(strLoopPath, true);
                    }
                }

                return Directory.EnumerateFileSystemEntries(strDestinationPath).Any();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                strError = ex.Message;
                return false;
            }
        }

        private static string GetZipRootPrefix(ZipArchive objZipArchive)
        {
            foreach (ZipArchiveEntry objEntry in objZipArchive.Entries)
            {
                string strFullName = objEntry.FullName;
                if (!strFullName.EndsWith("/manifest.xml", StringComparison.OrdinalIgnoreCase)
                    && !strFullName.EndsWith("manifest.xml", StringComparison.OrdinalIgnoreCase))
                    continue;

                int intSeparatorIndex = strFullName.IndexOf('/');
                if (intSeparatorIndex > 0)
                    return strFullName.Substring(0, intSeparatorIndex + 1);
                return string.Empty;
            }

            ZipArchiveEntry objFirstEntry = objZipArchive.Entries.FirstOrDefault();
            if (objFirstEntry == null)
                return string.Empty;

            int intFirstSeparatorIndex = objFirstEntry.FullName.IndexOf('/');
            return intFirstSeparatorIndex > 0
                ? objFirstEntry.FullName.Substring(0, intFirstSeparatorIndex + 1)
                : string.Empty;
        }

        private static bool TryFindCustomDataContentRoot(string strExtractPath, Guid guidExpected,
            CancellationToken token, out string strContentRoot, out string strManifestPath)
        {
            strContentRoot = string.Empty;
            strManifestPath = string.Empty;
            token.ThrowIfCancellationRequested();

            string strFallbackManifestPath = string.Empty;
            string strFallbackContentRoot = string.Empty;
            foreach (string strLoopManifestPath in Directory.EnumerateFiles(strExtractPath, "manifest.xml",
                         SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                string strLoopContentRoot = Path.GetDirectoryName(strLoopManifestPath);
                if (string.IsNullOrEmpty(strLoopContentRoot))
                    continue;

                if (TryGetManifestGuid(strLoopManifestPath, out Guid guidRemoteManifest, token)
                    && guidRemoteManifest == guidExpected)
                {
                    strContentRoot = strLoopContentRoot;
                    strManifestPath = strLoopManifestPath;
                    return true;
                }

                if (string.IsNullOrEmpty(strFallbackManifestPath))
                {
                    strFallbackManifestPath = strLoopManifestPath;
                    strFallbackContentRoot = strLoopContentRoot;
                }
            }

            if (string.IsNullOrEmpty(strFallbackManifestPath))
                return false;

            strContentRoot = strFallbackContentRoot;
            strManifestPath = strFallbackManifestPath;
            return true;
        }

        private static bool TryGetManifestGuid(string strManifestPath, out Guid guidManifest,
            CancellationToken token = default)
        {
            guidManifest = Guid.Empty;
            token.ThrowIfCancellationRequested();
            try
            {
                XPathDocument xmlObjManifest
                    = XPathDocumentExtensions.LoadStandardFromFilePatient(strManifestPath, token: token);
                XPathNavigator xmlNode = xmlObjManifest.CreateNavigator()
                                                       .SelectSingleNodeAndCacheExpression("manifest", token);
                return xmlNode != null && xmlNode.TryGetGuidFieldQuickly("guid", ref guidManifest);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Warn(ex, "Failed to read manifest GUID from {0}", strManifestPath);
                return false;
            }
        }

        private static bool TryGetManifestUpdateLocation(string strManifestPath, out string strUpdateLocation,
            CancellationToken token = default)
        {
            strUpdateLocation = string.Empty;
            token.ThrowIfCancellationRequested();
            try
            {
                XPathDocument xmlObjManifest
                    = XPathDocumentExtensions.LoadStandardFromFilePatient(strManifestPath, token: token);
                XPathNavigator xmlNode = xmlObjManifest.CreateNavigator()
                                                       .SelectSingleNodeAndCacheExpression("manifest", token);
                return xmlNode != null && xmlNode.TryGetStringFieldQuickly("updatelocation", ref strUpdateLocation);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Warn(ex, "Failed to read manifest updatelocation from {0}", strManifestPath);
                return false;
            }
        }

        private static void ReplaceDirectoryContents(string strSourcePath, string strDestinationPath,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!Directory.Exists(strDestinationPath))
                Directory.CreateDirectory(strDestinationPath);

            foreach (string strFile in Directory.EnumerateFiles(strDestinationPath, "*", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                File.Delete(strFile);
            }

            foreach (string strDirectory in Directory
                         .EnumerateDirectories(strDestinationPath, "*", SearchOption.AllDirectories)
                         .OrderByDescending(x => x.Length))
            {
                token.ThrowIfCancellationRequested();
                Directory.Delete(strDirectory);
            }

            foreach (string strSourceFile in Directory.EnumerateFiles(strSourcePath, "*", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                string strRelativePath = strSourceFile.Substring(strSourcePath.Length)
                                                      .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string strDestinationFile = Path.Combine(strDestinationPath, strRelativePath);
                string strDestinationDirectory = Path.GetDirectoryName(strDestinationFile);
                if (!string.IsNullOrEmpty(strDestinationDirectory))
                    Directory.CreateDirectory(strDestinationDirectory);
                File.Copy(strSourceFile, strDestinationFile, true);
            }
        }
    }
}
