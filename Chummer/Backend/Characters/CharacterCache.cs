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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace Chummer
{
    /// <summary>
    /// Caches a subset of a full character's properties for loading purposes.
    /// </summary>
    [DebuggerDisplay("{CharacterName} ({FileName})")]
    public sealed class CharacterCache : IHasLockObject
    {
        private string _strFilePath;
        private string _strFileName;
        private string _strErrorText;
        private string _strDescription;
        private string _strBackground;
        private string _strGameNotes;
        private string _strCharacterNotes;
        private string _strConcept;
        private string _strKarma;
        private string _strMetatype;
        private string _strMetavariant;
        private string _strPlayerName;
        private string _strCharacterName;
        private string _strCharacterAlias;
        private string _strBuildMethod;
        private string _strEssence;
        private Image _imgMugshot;
        private int _intCreated;
        private string _strSettingsFile;
        private readonly ConcurrentDictionary<string, object> _dicMyPluginData = new ConcurrentDictionary<string, object>();
        private Task<string> _tskRunningDownloadTask;
        private EventHandler _onMyDoubleClick;
        private EventHandler _onMyContextMenuDeleteClick;
        private EventHandler<TreeViewEventArgs> _onMyAfterSelect;
        private EventHandler<Tuple<KeyEventArgs, TreeNode>> _onMyKeyDown;

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public string FilePath
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFilePath;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strFilePath, value);
            }
        }

        public string FileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFileName;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strFileName, value);
            }
        }

        public string ErrorText
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strErrorText;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strErrorText, value);
            }
        }

        public string Description
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDescription;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strDescription, value);
            }
        }

        public string Background
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBackground;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strBackground, value);
            }
        }

        public string GameNotes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strGameNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strGameNotes, value);
            }
        }

        public string CharacterNotes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strCharacterNotes, value);
            }
        }

        public string Concept
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strConcept;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strConcept, value);
            }
        }

        public string Karma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strKarma;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strKarma, value);
            }
        }

        public string Metatype
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMetatype;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strMetatype, value);
            }
        }

        public string Metavariant
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMetavariant;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strMetavariant, value);
            }
        }

        public string PlayerName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPlayerName;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strPlayerName, value);
            }
        }

        public string CharacterName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterName;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strCharacterName, value);
            }
        }

        public string CharacterAlias
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterAlias;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strCharacterAlias, value);
            }
        }

        public string BuildMethod
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBuildMethod;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strBuildMethod, value);
            }
        }

        public string Essence
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strEssence;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strEssence, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Image Mugshot
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _imgMugshot;
            }
            private set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _imgMugshot, value)?.Dispose();
            }
        }

        public bool Created
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intCreated > 0;
            }
            set
            {
                int intNewValue = value.ToInt32();
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _intCreated, intNewValue);
            }
        }

        public string SettingsFile
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSettingsFile;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _strSettingsFile, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public ConcurrentDictionary<string, object> MyPluginDataDic
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicMyPluginData;
            }
        }

        public Task<string> RunningDownloadTask
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _tskRunningDownloadTask;
            }
            set
            {
                Task<string> tskOld;
                using (LockObject.EnterReadLock())
                    tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, value);
                if (tskOld != null && tskOld != value)
                    Utils.SafelyRunSynchronously(() => tskOld);
            }
        }

        public CharacterCache()
        {
            SetDefaultEventHandlers();
        }

        public CharacterCache(CharacterCache objExistingCache) : this()
        {
            CopyFrom(objExistingCache);
        }

        /// <summary>
        /// Syntactic sugar to call LoadFromFile() synchronously immediately after the constructor.
        /// </summary>
        public CharacterCache(string strFile) : this()
        {
            LoadFromFile(strFile);
        }

        /// <summary>
        /// Syntactic sugar to call CopyFrom() asynchronously immediately after the constructor.
        /// </summary>
        public static async Task<CharacterCache> CreateFromFileAsync(CharacterCache objExistingCache, CancellationToken token = default)
        {
            CharacterCache objReturn = new CharacterCache();
            await objReturn.CopyFromAsync(objExistingCache, token).ConfigureAwait(false);
            return objReturn;
        }

        /// <summary>
        /// Syntactic sugar to call LoadFromFile() asynchronously immediately after the constructor.
        /// </summary>
        public static async Task<CharacterCache> CreateFromFileAsync(string strFile, CancellationToken token = default)
        {
            CharacterCache objReturn = new CharacterCache();
            await objReturn.LoadFromFileAsync(strFile, token).ConfigureAwait(false);
            return objReturn;
        }

        public void CopyFrom(CharacterCache objExistingCache)
        {
            using (LockObject.EnterWriteLock())
            using (objExistingCache.LockObject.EnterReadLock())
            {
                _strBackground = objExistingCache.Background;
                _strBuildMethod = objExistingCache.BuildMethod;
                _strCharacterAlias = objExistingCache.CharacterAlias;
                _strCharacterName = objExistingCache.CharacterName;
                _strCharacterNotes = objExistingCache.CharacterNotes;
                _strConcept = objExistingCache.Concept;
                _intCreated = objExistingCache.Created.ToInt32();
                _strDescription = objExistingCache.Description;
                _strEssence = objExistingCache.Essence;
                _strGameNotes = objExistingCache.GameNotes;
                _strKarma = objExistingCache.Karma;
                _strFileName = objExistingCache.FileName;
                _strMetatype = objExistingCache.Metatype;
                _strMetavariant = objExistingCache.Metavariant;
                _strPlayerName = objExistingCache.PlayerName;
                _strSettingsFile = objExistingCache.SettingsFile;
                Interlocked.Exchange(ref _imgMugshot, objExistingCache.Mugshot.Clone() as Image)?.Dispose();
            }
        }

        public async Task CopyFromAsync(CharacterCache objExistingCache, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await objExistingCache.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _strBackground = objExistingCache.Background;
                    _strBuildMethod = objExistingCache.BuildMethod;
                    _strCharacterAlias = objExistingCache.CharacterAlias;
                    _strCharacterName = objExistingCache.CharacterName;
                    _strCharacterNotes = objExistingCache.CharacterNotes;
                    _strConcept = objExistingCache.Concept;
                    _intCreated = objExistingCache.Created.ToInt32();
                    _strDescription = objExistingCache.Description;
                    _strEssence = objExistingCache.Essence;
                    _strGameNotes = objExistingCache.GameNotes;
                    _strKarma = objExistingCache.Karma;
                    _strFileName = objExistingCache.FileName;
                    _strMetatype = objExistingCache.Metatype;
                    _strMetavariant = objExistingCache.Metavariant;
                    _strPlayerName = objExistingCache.PlayerName;
                    _strSettingsFile = objExistingCache.SettingsFile;
                    Interlocked.Exchange(ref _imgMugshot, objExistingCache.Mugshot.Clone() as Image)?.Dispose();
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void SetDefaultEventHandlers()
        {
            using (LockObject.EnterWriteLock())
            {
                _onMyDoubleClick += OnDefaultDoubleClick;
                _onMyKeyDown += OnDefaultKeyDown;
                _onMyContextMenuDeleteClick += OnDefaultContextMenuDeleteClick;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyDoubleClick
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyDoubleClick;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _onMyDoubleClick, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyContextMenuDeleteClick
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyContextMenuDeleteClick;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _onMyContextMenuDeleteClick, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<TreeViewEventArgs> OnMyAfterSelect
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyAfterSelect;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _onMyAfterSelect, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<Tuple<KeyEventArgs, TreeNode>> OnMyKeyDown
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyKeyDown;
            }
            set
            {
                using (LockObject.EnterReadLock())
                    Interlocked.Exchange(ref _onMyKeyDown, value);
            }
        }

        public async void OnDefaultDoubleClick(object sender, EventArgs e)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync().ConfigureAwait(false);
            try
            {
                Character objOpenCharacter = await Program.OpenCharacters
                    .FirstOrDefaultAsync(x => string.Equals(x.FileName, FileName, StringComparison.Ordinal))
                    .ConfigureAwait(false);
                if (objOpenCharacter == null)
                {
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program
                               .CreateAndShowProgressBarAsync(
                                   FilePath, Character.NumLoadingSections)
                               .ConfigureAwait(false))
                        objOpenCharacter = await Program
                            .LoadCharacterAsync(FilePath, frmLoadingBar: frmLoadingBar.MyForm)
                            .ConfigureAwait(false);
                }

                if (!await Program.SwitchToOpenCharacter(objOpenCharacter).ConfigureAwait(false))
                    await Program.OpenCharacter(objOpenCharacter).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void OnDefaultContextMenuDeleteClick(object sender, EventArgs e)
        {
            if (sender is TreeNode t)
            {
                switch (t.Parent.Tag?.ToString())
                {
                    case "Recent":
                        GlobalSettings.MostRecentlyUsedCharacters.Remove(FilePath);
                        break;

                    case "Favorite":
                        GlobalSettings.FavoriteCharacters.Remove(FilePath);
                        break;
                }
            }
        }

        public bool LoadFromFile(string strFile)
        {
            return Utils.SafelyRunSynchronously(() => LoadFromFileCoreAsync(true, strFile));
        }

        public Task<bool> LoadFromFileAsync(string strFile, CancellationToken token = default)
        {
            return LoadFromFileCoreAsync(false, strFile, token);
        }

        private async Task<bool> LoadFromFileCoreAsync(bool blnSync, string strFile, CancellationToken token = default)
        {
            IDisposable objSyncLocker = null;
            IAsyncDisposable objLocker = null;
            if (blnSync)
                objSyncLocker = LockObject.EnterWriteLock(token);
            else
                objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Task<string> tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, null);
                if (tskOld != null)
                {
                    try
                    {
                        if (blnSync)
                            Utils.SafelyRunSynchronously(() => tskOld, token);
                        else
                            await tskOld.ConfigureAwait(false);
                    }
                    catch
                    {
                        _ = Interlocked.CompareExchange(ref _tskRunningDownloadTask, tskOld, null);
                        throw;
                    }
                }
                string strErrorText = string.Empty;
                XPathNavigator xmlSourceNode;
                if (!File.Exists(strFile))
                {
                    xmlSourceNode = null;
                    strErrorText = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? LanguageManager.GetString("MessageTitle_FileNotFound", token: token)
                        : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                               .ConfigureAwait(false);
                }
                else
                {
                    // If we run into any problems loading the character cache, fail out early.
                    try
                    {
                        XPathDocument xmlDoc = blnSync
                            ? LoadXPathDocument()
                            : await Task.Run(LoadXPathDocumentAsync, token).ConfigureAwait(false);

                        XPathDocument LoadXPathDocument()
                        {
                            if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromFile(strFile, token: token);
                            if (strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromLzmaCompressedFile(strFile, token: token);
                            Utils.BreakIfDebug();
                            throw new InvalidOperationException();
                        }

                        Task<XPathDocument> LoadXPathDocumentAsync()
                        {
                            if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromFileAsync(strFile, token: token);
                            if (strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromLzmaCompressedFileAsync(
                                    strFile, token: token);
                            Utils.BreakIfDebug();
                            return Task.FromException<XPathDocument>(new InvalidOperationException());
                        }

                        xmlSourceNode = xmlDoc.CreateNavigator().SelectSingleNodeAndCacheExpression("/character", token);
                    }
                    catch (Exception ex)
                    {
                        xmlSourceNode = null;
                        strErrorText = ex.ToString();
                    }
                }

                if (xmlSourceNode != null)
                {
                    _strDescription = xmlSourceNode.SelectSingleNodeAndCacheExpression("description", token)?.Value;
                    _strBuildMethod = xmlSourceNode.SelectSingleNodeAndCacheExpression("buildmethod", token)?.Value;
                    _strBackground = xmlSourceNode.SelectSingleNodeAndCacheExpression("background", token)?.Value;
                    _strCharacterNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("notes", token)?.Value;
                    _strGameNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("gamenotes", token)?.Value;
                    _strConcept = xmlSourceNode.SelectSingleNodeAndCacheExpression("concept", token)?.Value;
                    _strKarma = xmlSourceNode.SelectSingleNodeAndCacheExpression("totalkarma", token)?.Value;
                    _strMetatype = xmlSourceNode.SelectSingleNodeAndCacheExpression("metatype", token)?.Value;
                    _strMetavariant = xmlSourceNode.SelectSingleNodeAndCacheExpression("metavariant", token)?.Value;
                    _strPlayerName = xmlSourceNode.SelectSingleNodeAndCacheExpression("playername", token)?.Value;
                    _strCharacterName = xmlSourceNode.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                    _strCharacterAlias = xmlSourceNode.SelectSingleNodeAndCacheExpression("alias", token)?.Value;
                    _intCreated = (xmlSourceNode.SelectSingleNodeAndCacheExpression("created", token)?.Value
                                   == bool.TrueString).ToInt32();
                    _strEssence = xmlSourceNode.SelectSingleNodeAndCacheExpression("totaless", token)?.Value;

                    string strSettings
                        = xmlSourceNode.SelectSingleNodeAndCacheExpression("settings", token)?.Value
                          ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSettings))
                    {
                        if ((await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false))
                            .TryGetValue(strSettings, out CharacterSettings objSettings))
                            _strSettingsFile = blnSync
                                ? objSettings.CurrentDisplayName
                                : await objSettings.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        else
                        {
                            string strTemp = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("MessageTitle_FileNotFound", token: token) +
                                  // ReSharper disable once MethodHasAsyncOverload
                                  LanguageManager.GetString("String_Space", token: token)
                                : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                                       .ConfigureAwait(false) +
                                  await LanguageManager.GetStringAsync("String_Space", token: token)
                                                       .ConfigureAwait(false);
                            _strSettingsFile = strTemp + '[' + strSettings + ']';
                        }
                    }
                    else
                        _strSettingsFile = string.Empty;

                    string strMugshotBase64
                        = xmlSourceNode.SelectSingleNodeAndCacheExpression("mugshot", token)?.Value
                          ?? string.Empty;
                    if (string.IsNullOrEmpty(strMugshotBase64))
                    {
                        XPathNavigator xmlMainMugshotIndex = xmlSourceNode.SelectSingleNodeAndCacheExpression("mainmugshotindex", token);
                        if (xmlMainMugshotIndex != null &&
                            int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) &&
                            intMainMugshotIndex >= 0)
                        {
                            XPathNodeIterator xmlMugshotList = xmlSourceNode.SelectAndCacheExpression("mugshots/mugshot", token);
                            if (xmlMugshotList.Count > intMainMugshotIndex)
                            {
                                int intIndex = 0;
                                foreach (XPathNavigator xmlMugshot in xmlMugshotList)
                                {
                                    if (intMainMugshotIndex == intIndex)
                                    {
                                        strMugshotBase64 = xmlMugshot.Value;
                                        break;
                                    }

                                    ++intIndex;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(strMugshotBase64))
                    {
                        Image imgNewMugshot;
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            using (Image imgMugshot = strMugshotBase64.ToImage())
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                imgNewMugshot = imgMugshot.GetCompressedImage();
                        }
                        else
                        {
                            using (Image imgMugshot
                                   = await strMugshotBase64.ToImageAsync(token: token).ConfigureAwait(false))
                                imgNewMugshot = await imgMugshot.GetCompressedImageAsync(token: token)
                                                                .ConfigureAwait(false);
                        }
                        Interlocked.Exchange(ref _imgMugshot, imgNewMugshot)?.Dispose();
                    }
                }
                else
                {
                    _strErrorText = strErrorText;
                }

                _strFilePath = strFile;
                if (!string.IsNullOrEmpty(strFile))
                {
                    int last = strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    if (strFile.Length > last)
                        _strFileName = strFile.Substring(last);
                }

                return string.IsNullOrEmpty(strErrorText);
            }
            finally
            {
                if (blnSync)
                    objSyncLocker.Dispose();
                else
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="blnAddMarkerIfOpen">Whether to add an asterisk to the beginning of the name if the character is open.</param>
        /// <returns></returns>
        public string CalculatedName(bool blnAddMarkerIfOpen = true)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strReturn;
            using (LockObject.EnterReadLock())
            {
                if (!string.IsNullOrEmpty(ErrorText))
                {
                    strReturn = Path.GetFileNameWithoutExtension(FileName) + strSpace + '(' +
                                LanguageManager.GetString("String_Error") + ')';
                }
                else
                {
                    strReturn = CharacterAlias;
                    if (string.IsNullOrEmpty(strReturn))
                    {
                        strReturn = CharacterName;
                        if (string.IsNullOrEmpty(strReturn))
                            strReturn = LanguageManager.GetString("String_UnnamedCharacter");
                    }

                    string strBuildMethod = LanguageManager.GetString("String_" + BuildMethod, false);
                    if (string.IsNullOrEmpty(strBuildMethod))
                        strBuildMethod = LanguageManager.GetString("String_Unknown");
                    strReturn += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                                 + LanguageManager.GetString(Created ? "Title_CareerMode" : "Title_CreateMode") + ')';
                }

                if (blnAddMarkerIfOpen && Program.MainForm != null)
                {
                    string strMarker = string.Empty;
                    if (Program.MainForm.OpenCharacterEditorForms?.Any(
                            x => !x.CharacterObject.IsDisposed && string.Equals(x.CharacterObject.FileName, FilePath,
                                StringComparison.Ordinal)) == true)
                        strMarker += '*';
                    if (Program.MainForm.OpenCharacterSheetViewers?.Any(
                            x => x.CharacterObjects.Any(y =>
                                !y.IsDisposed && string.Equals(y.FileName, FilePath,
                                    StringComparison.Ordinal))) == true)
                        strMarker += '^';
                    if (Program.MainForm.OpenCharacterExportForms?.Any(
                            x => !x.CharacterObject.IsDisposed && string.Equals(x.CharacterObject.FileName, FilePath,
                                StringComparison.Ordinal)) == true)
                        strMarker += '\'';
                    if (!string.IsNullOrEmpty(strMarker))
                        strReturn = strMarker + strSpace + strReturn;
                }
            }

            return strReturn;
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="blnAddMarkerIfOpen">Whether to add an asterisk to the beginning of the name if the character is open.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<string> CalculatedNameAsync(bool blnAddMarkerIfOpen = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strReturn;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(ErrorText))
                {
                    strReturn = Path.GetFileNameWithoutExtension(FileName) + strSpace + '(' +
                                await LanguageManager.GetStringAsync("String_Error", token: token)
                                    .ConfigureAwait(false) + ')';
                }
                else
                {
                    strReturn = CharacterAlias;
                    if (string.IsNullOrEmpty(strReturn))
                    {
                        strReturn = CharacterName;
                        if (string.IsNullOrEmpty(strReturn))
                            strReturn = await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token)
                                .ConfigureAwait(false);
                    }

                    string strBuildMethod = await LanguageManager.GetStringAsync("String_" + BuildMethod, false, token)
                        .ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strBuildMethod))
                        strBuildMethod = await LanguageManager.GetStringAsync("String_Unknown", token: token)
                            .ConfigureAwait(false);
                    strReturn += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                                 + await LanguageManager
                                     .GetStringAsync(Created ? "Title_CareerMode" : "Title_CreateMode", token: token)
                                     .ConfigureAwait(false) + ')';
                }

                if (blnAddMarkerIfOpen && Program.MainForm != null)
                {
                    string strMarker = string.Empty;
                    ThreadSafeObservableCollection<CharacterShared> lstToProcess1
                        = Program.MainForm.OpenCharacterEditorForms;
                    if (lstToProcess1 != null && await lstToProcess1
                            .AnyAsync(
                                async x => !x.CharacterObject.IsDisposed &&
                                           string.Equals(
                                               await x.CharacterObject.GetFileNameAsync(token).ConfigureAwait(false),
                                               FilePath, StringComparison.Ordinal), token)
                            .ConfigureAwait(false))
                        strMarker += '*';
                    ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess2
                        = Program.MainForm.OpenCharacterSheetViewers;
                    if (lstToProcess1 != null && await lstToProcess2
                            .AnyAsync(
                                x => x.CharacterObjects.AnyAsync(
                                    async y => !y.IsDisposed && string.Equals(
                                        await y.GetFileNameAsync(token).ConfigureAwait(false), FilePath,
                                        StringComparison.Ordinal), token), token).ConfigureAwait(false))
                        strMarker += '^';
                    ThreadSafeObservableCollection<ExportCharacter> lstToProcess3
                        = Program.MainForm.OpenCharacterExportForms;
                    if (lstToProcess1 != null && await lstToProcess3
                            .AnyAsync(
                                async x => !x.CharacterObject.IsDisposed &&
                                           string.Equals(
                                               await x.CharacterObject.GetFileNameAsync(token).ConfigureAwait(false),
                                               FilePath, StringComparison.Ordinal), token)
                            .ConfigureAwait(false))
                        strMarker += '\'';
                    if (!string.IsNullOrEmpty(strMarker))
                        strReturn = strMarker + strSpace + strReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return strReturn;
        }

        public void OnDefaultKeyDown(object sender, Tuple<KeyEventArgs, TreeNode> args)
        {
            if (args?.Item1.KeyCode == Keys.Delete)
            {
                switch (args.Item2.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalSettings.MostRecentlyUsedCharacters.Remove(FilePath);
                        break;

                    case "Favorite":
                        GlobalSettings.FavoriteCharacters.Remove(FilePath);
                        break;
                }
            }
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            using (LockObject.EnterWriteLock())
            {
                Interlocked.Exchange(ref _imgMugshot, null)?.Dispose();
                Task<string> tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, null);
                if (tskOld != null)
                    Utils.SafelyRunSynchronously(() => tskOld);
            }

            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Interlocked.Exchange(ref _imgMugshot, null)?.Dispose();
                Task<string> tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, null);
                if (tskOld != null)
                    await tskOld.ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
