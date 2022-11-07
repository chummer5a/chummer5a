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
        private readonly LockingDictionary<string, object> _dicMyPluginData = new LockingDictionary<string, object>();
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
                using (EnterReadLock.Enter(LockObject))
                    return _strFilePath;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strFilePath, value);
            }
        }

        public string FileName
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strFileName;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strFileName, value);
            }
        }

        public string ErrorText
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strErrorText;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strErrorText, value);
            }
        }

        public string Description
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strDescription;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strDescription, value);
            }
        }

        public string Background
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strBackground;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strBackground, value);
            }
        }

        public string GameNotes
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strGameNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strGameNotes, value);
            }
        }

        public string CharacterNotes
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strCharacterNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strCharacterNotes, value);
            }
        }

        public string Concept
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strConcept;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strConcept, value);
            }
        }

        public string Karma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strKarma;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strKarma, value);
            }
        }

        public string Metatype
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strMetatype;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strMetatype, value);
            }
        }

        public string Metavariant
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strMetavariant;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strMetavariant, value);
            }
        }

        public string PlayerName
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strPlayerName;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strPlayerName, value);
            }
        }

        public string CharacterName
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strCharacterName;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strCharacterName, value);
            }
        }

        public string CharacterAlias
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strCharacterAlias;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strCharacterAlias, value);
            }
        }

        public string BuildMethod
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strBuildMethod;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strBuildMethod, value);
            }
        }

        public string Essence
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strEssence;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _imgMugshot;
            }
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _imgMugshot, value)?.Dispose();
            }
        }

        public bool Created
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intCreated > 0;
            }
            set
            {
                int intNewValue = value.ToInt32();
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _intCreated, intNewValue);
            }
        }

        public string SettingsFile
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strSettingsFile;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _strSettingsFile, value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public LockingDictionary<string, object> MyPluginDataDic
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicMyPluginData;
            }
        }

        public Task<string> RunningDownloadTask
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _tskRunningDownloadTask;
            }
            set
            {
                Task<string> tskOld;
                using (EnterReadLock.Enter(LockObject))
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
            using (EnterReadLock.Enter(objExistingCache.LockObject))
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
                using (await EnterReadLock.EnterAsync(objExistingCache.LockObject, token).ConfigureAwait(false))
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
                using (EnterReadLock.Enter(LockObject))
                    return _onMyDoubleClick;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _onMyContextMenuDeleteClick;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _onMyAfterSelect;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _onMyKeyDown;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                    Interlocked.Exchange(ref _onMyKeyDown, value);
            }
        }

        public async void OnDefaultDoubleClick(object sender, EventArgs e)
        {
            Character objOpenCharacter = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == FileName)
                                                      .ConfigureAwait(false);
            if (objOpenCharacter == null)
            {
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program
                                                                        .CreateAndShowProgressBarAsync(
                                                                            FilePath, Character.NumLoadingSections)
                                                                        .ConfigureAwait(false))
                    objOpenCharacter = await Program.LoadCharacterAsync(FilePath, frmLoadingBar: frmLoadingBar.MyForm)
                                                    .ConfigureAwait(false);
            }

            if (!await Program.SwitchToOpenCharacter(objOpenCharacter).ConfigureAwait(false))
                await Program.OpenCharacter(objOpenCharacter).ConfigureAwait(false);
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
                _tskRunningDownloadTask = null;
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
                                return XPathDocumentExtensions.LoadStandardFromFile(strFile);
                            if (strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromLzmaCompressedFile(strFile);
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

                        xmlSourceNode = blnSync
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ? xmlDoc.CreateNavigator().SelectSingleNodeAndCacheExpression("/character")
                            : await xmlDoc.CreateNavigator()
                                          .SelectSingleNodeAndCacheExpressionAsync("/character", token: token)
                                          .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        xmlSourceNode = null;
                        strErrorText = ex.ToString();
                    }
                }

                if (xmlSourceNode != null)
                {
                    if (blnSync)
                    {
                        // ReSharper disable MethodHasAsyncOverloadWithCancellation
                        _strDescription = xmlSourceNode.SelectSingleNodeAndCacheExpression("description")?.Value;
                        _strBuildMethod = xmlSourceNode.SelectSingleNodeAndCacheExpression("buildmethod")?.Value;
                        _strBackground = xmlSourceNode.SelectSingleNodeAndCacheExpression("background")?.Value;
                        _strCharacterNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("notes")?.Value;
                        _strGameNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("gamenotes")?.Value;
                        _strConcept = xmlSourceNode.SelectSingleNodeAndCacheExpression("concept")?.Value;
                        _strKarma = xmlSourceNode.SelectSingleNodeAndCacheExpression("totalkarma")?.Value;
                        _strMetatype = xmlSourceNode.SelectSingleNodeAndCacheExpression("metatype")?.Value;
                        _strMetavariant = xmlSourceNode.SelectSingleNodeAndCacheExpression("metavariant")?.Value;
                        _strPlayerName = xmlSourceNode.SelectSingleNodeAndCacheExpression("playername")?.Value;
                        _strCharacterName = xmlSourceNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                        _strCharacterAlias = xmlSourceNode.SelectSingleNodeAndCacheExpression("alias")?.Value;
                        _intCreated = (xmlSourceNode.SelectSingleNodeAndCacheExpression("created")?.Value
                                       == bool.TrueString).ToInt32();
                        _strEssence = xmlSourceNode.SelectSingleNodeAndCacheExpression("totaless")?.Value;
                        // ReSharper restore MethodHasAsyncOverloadWithCancellation
                    }
                    else
                    {
                        _strDescription = (await xmlSourceNode
                                                 .SelectSingleNodeAndCacheExpressionAsync("description", token: token)
                                                 .ConfigureAwait(false))?.Value;
                        _strBuildMethod = (await xmlSourceNode
                                                 .SelectSingleNodeAndCacheExpressionAsync("buildmethod", token: token)
                                                 .ConfigureAwait(false))?.Value;
                        _strBackground = (await xmlSourceNode
                                                .SelectSingleNodeAndCacheExpressionAsync("background", token: token)
                                                .ConfigureAwait(false))?.Value;
                        _strCharacterNotes = (await xmlSourceNode
                                                    .SelectSingleNodeAndCacheExpressionAsync("notes", token: token)
                                                    .ConfigureAwait(false))?.Value;
                        _strGameNotes = (await xmlSourceNode
                                               .SelectSingleNodeAndCacheExpressionAsync("gamenotes", token: token)
                                               .ConfigureAwait(false))?.Value;
                        _strConcept = (await xmlSourceNode
                                             .SelectSingleNodeAndCacheExpressionAsync("concept", token: token)
                                             .ConfigureAwait(false))?.Value;
                        _strKarma = (await xmlSourceNode
                                           .SelectSingleNodeAndCacheExpressionAsync("totalkarma", token: token)
                                           .ConfigureAwait(false))?.Value;
                        _strMetatype = (await xmlSourceNode
                                              .SelectSingleNodeAndCacheExpressionAsync("metatype", token: token)
                                              .ConfigureAwait(false))?.Value;
                        _strMetavariant = (await xmlSourceNode
                                                 .SelectSingleNodeAndCacheExpressionAsync("metavariant", token: token)
                                                 .ConfigureAwait(false))?.Value;
                        _strPlayerName = (await xmlSourceNode
                                                .SelectSingleNodeAndCacheExpressionAsync("playername", token: token)
                                                .ConfigureAwait(false))?.Value;
                        _strCharacterName = (await xmlSourceNode
                                                   .SelectSingleNodeAndCacheExpressionAsync("name", token: token)
                                                   .ConfigureAwait(false))?.Value;
                        _strCharacterAlias = (await xmlSourceNode
                                                    .SelectSingleNodeAndCacheExpressionAsync("alias", token: token)
                                                    .ConfigureAwait(false))?.Value;
                        _intCreated = ((await xmlSourceNode
                                              .SelectSingleNodeAndCacheExpressionAsync("created", token: token)
                                              .ConfigureAwait(false))?.Value == bool.TrueString).ToInt32();
                        _strEssence = (await xmlSourceNode
                                             .SelectSingleNodeAndCacheExpressionAsync("totaless", token: token)
                                             .ConfigureAwait(false))?.Value;
                    }

                    string strSettings
                        = (blnSync
                              // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                              ? xmlSourceNode.SelectSingleNodeAndCacheExpression("settings")
                              : await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("settings", token: token)
                                                   .ConfigureAwait(false))?.Value
                          ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSettings))
                    {
                        (bool blnSuccess, CharacterSettings objSettings)
                            = await (await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false))
                                    .TryGetValueAsync(strSettings, token).ConfigureAwait(false);
                        if (blnSuccess)
                            _strSettingsFile = objSettings.DisplayName;
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
                        = (blnSync
                              // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                              ? xmlSourceNode.SelectSingleNodeAndCacheExpression("mugshot")
                              : await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("mugshot", token: token)
                                                   .ConfigureAwait(false))?.Value
                          ?? string.Empty;
                    if (string.IsNullOrEmpty(strMugshotBase64))
                    {
                        XPathNavigator xmlMainMugshotIndex = blnSync
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ? xmlSourceNode.SelectSingleNodeAndCacheExpression("mainmugshotindex")
                            : await xmlSourceNode
                                    .SelectSingleNodeAndCacheExpressionAsync("mainmugshotindex", token: token)
                                    .ConfigureAwait(false);
                        if (xmlMainMugshotIndex != null &&
                            int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) &&
                            intMainMugshotIndex >= 0)
                        {
                            XPathNodeIterator xmlMugshotList = blnSync
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ? xmlSourceNode.SelectAndCacheExpression("mugshots/mugshot")
                                : await xmlSourceNode.SelectAndCacheExpressionAsync("mugshots/mugshot", token: token)
                                                     .ConfigureAwait(false);
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
            if (!string.IsNullOrEmpty(ErrorText))
            {
                strReturn = Path.GetFileNameWithoutExtension(FileName) + strSpace + '(' + LanguageManager.GetString("String_Error") + ')';
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
                if (Program.MainForm.OpenCharacterEditorForms.Any(
                        x => !x.CharacterObject.IsDisposed && x.CharacterObject.FileName == FilePath))
                    strMarker += '*';
                if (Program.MainForm.OpenCharacterSheetViewers.Any(
                        x => x.CharacterObjects.Any(y => !y.IsDisposed && y.FileName == FilePath)))
                    strMarker += '^';
                if (Program.MainForm.OpenCharacterExportForms.Any(
                        x => !x.CharacterObject.IsDisposed && x.CharacterObject.FileName == FilePath))
                    strMarker += '\'';
                if (!string.IsNullOrEmpty(strMarker))
                    strReturn = strMarker + strSpace + strReturn;
            }

            return strReturn;
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="blnAddMarkerIfOpen">Whether to add an asterisk to the beginning of the name if the character is open.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async ValueTask<string> CalculatedNameAsync(bool blnAddMarkerIfOpen = true, CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strReturn;
            if (!string.IsNullOrEmpty(ErrorText))
            {
                strReturn = Path.GetFileNameWithoutExtension(FileName) + strSpace + '(' + await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false) + ')';
            }
            else
            {
                strReturn = CharacterAlias;
                if (string.IsNullOrEmpty(strReturn))
                {
                    strReturn = CharacterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token).ConfigureAwait(false);
                }

                string strBuildMethod = await LanguageManager.GetStringAsync("String_" + BuildMethod, false, token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strBuildMethod))
                    strBuildMethod = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                strReturn += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                             + await LanguageManager.GetStringAsync(Created ? "Title_CareerMode" : "Title_CreateMode", token: token).ConfigureAwait(false) + ')';
            }

            if (blnAddMarkerIfOpen && Program.MainForm != null)
            {
                string strMarker = string.Empty;
                if (await Program.MainForm.OpenCharacterEditorForms.AnyAsync(
                        x => !x.CharacterObject.IsDisposed && x.CharacterObject.FileName == FilePath, token).ConfigureAwait(false))
                    strMarker += '*';
                if (await Program.MainForm.OpenCharacterSheetViewers.AnyAsync(
                        x => x.CharacterObjects.Any(y => !y.IsDisposed && y.FileName == FilePath), token).ConfigureAwait(false))
                    strMarker += '^';
                if (await Program.MainForm.OpenCharacterExportForms.AnyAsync(
                        x => !x.CharacterObject.IsDisposed && x.CharacterObject.FileName == FilePath, token).ConfigureAwait(false))
                    strMarker += '\'';
                if (!string.IsNullOrEmpty(strMarker))
                    strReturn = strMarker + strSpace + strReturn;
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
                Task tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, null);
                if (tskOld != null)
                    Utils.SafelyRunSynchronously(() => tskOld);
                _dicMyPluginData.Dispose();
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
                Task tskOld = Interlocked.Exchange(ref _tskRunningDownloadTask, null);
                if (tskOld != null)
                    await tskOld.ConfigureAwait(false);
                await _dicMyPluginData.DisposeAsync().ConfigureAwait(false);
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
