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
using static Chummer.EventHandlerExtensions;

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
        private SafeAsyncEventHandler _onMyDoubleClick;
        private SafeAsyncEventHandler _onMyContextMenuDeleteClick;
        private SafeAsyncEventHandler<TreeViewEventArgs> _onMyAfterSelect;
        private SafeAsyncEventHandler<ValueTuple<KeyEventArgs, TreeNode>> _onMyKeyDown;

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public string FilePath
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFilePath;
            }
        }

        public async Task<string> GetFilePathAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strFilePath;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string FileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFileName;
            }
        }

        public async Task<string> GetFileNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strFileName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string ErrorText
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strErrorText;
            }
        }

        public async Task<string> GetErrorTextAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strErrorText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Description
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDescription;
            }
        }

        public async Task<string> GetDescriptionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strDescription;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Background
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBackground;
            }
        }

        public async Task<string> GetBackgroundAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBackground;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string GameNotes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strGameNotes;
            }
        }

        public async Task<string> GetGameNotesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strGameNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CharacterNotes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterNotes;
            }
        }

        public async Task<string> GetCharacterNotesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCharacterNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Concept
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strConcept;
            }
        }

        public async Task<string> GetConceptAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strConcept;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Karma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strKarma;
            }
        }

        public async Task<string> GetKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strKarma;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Metatype
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMetatype;
            }
        }

        public async Task<string> GetMetatypeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strMetatype;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Metavariant
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMetavariant;
            }
        }

        public async Task<string> GetMetavariantAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strMetavariant;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string PlayerName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPlayerName;
            }
        }

        public async Task<string> GetPlayerNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strPlayerName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CharacterName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterName;
            }
        }

        public async Task<string> GetCharacterNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCharacterName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CharacterAlias
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCharacterAlias;
            }
        }

        public async Task<string> GetCharacterAliasAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCharacterAlias;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string BuildMethod
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBuildMethod;
            }
        }

        public async Task<string> GetBuildMethodAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBuildMethod;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string Essence
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strEssence;
            }
        }

        public async Task<string> GetEssenceAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strEssence;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
        }

        public async Task<Image> GetMugshotAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _imgMugshot;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Created
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intCreated > 0;
            }
        }

        public async Task<bool> GetCreatedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intCreated > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string SettingsFile
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSettingsFile;
            }
        }

        public async Task<string> GetSettingsFileAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strSettingsFile;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
            try
            {
                await objReturn.CopyFromAsync(objExistingCache, token).ConfigureAwait(false);
                return objReturn;
            }
            catch
            {
                await objReturn.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Syntactic sugar to call LoadFromFile() asynchronously immediately after the constructor.
        /// </summary>
        public static async Task<CharacterCache> CreateFromFileAsync(string strFile, CancellationToken token = default)
        {
            CharacterCache objReturn = new CharacterCache();
            try
            {
                await objReturn.LoadFromFileAsync(strFile, token).ConfigureAwait(false);
                return objReturn;
            }
            catch
            {
                await objReturn.DisposeAsync().ConfigureAwait(false);
                throw;
            }
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
                    _strBackground = await objExistingCache.GetBackgroundAsync(token).ConfigureAwait(false);
                    _strBuildMethod = await objExistingCache.GetBuildMethodAsync(token).ConfigureAwait(false);
                    _strCharacterAlias = await objExistingCache.GetCharacterAliasAsync(token).ConfigureAwait(false);
                    _strCharacterName = await objExistingCache.GetCharacterNameAsync(token).ConfigureAwait(false);
                    _strCharacterNotes = await objExistingCache.GetCharacterNotesAsync(token).ConfigureAwait(false);
                    _strConcept = await objExistingCache.GetConceptAsync(token).ConfigureAwait(false);
                    _intCreated = (await objExistingCache.GetCreatedAsync(token).ConfigureAwait(false)).ToInt32();
                    _strDescription = await objExistingCache.GetDescriptionAsync(token).ConfigureAwait(false);
                    _strEssence = await objExistingCache.GetEssenceAsync(token).ConfigureAwait(false);
                    _strGameNotes = await objExistingCache.GetGameNotesAsync(token).ConfigureAwait(false);
                    _strKarma = await objExistingCache.GetKarmaAsync(token).ConfigureAwait(false);
                    _strFileName = await objExistingCache.GetFileNameAsync(token).ConfigureAwait(false);
                    _strMetatype = await objExistingCache.GetMetatypeAsync(token).ConfigureAwait(false);
                    _strMetavariant = await objExistingCache.GetMetavariantAsync(token).ConfigureAwait(false);
                    _strPlayerName = await objExistingCache.GetPlayerNameAsync(token).ConfigureAwait(false);
                    _strSettingsFile = await objExistingCache.GetSettingsFileAsync(token).ConfigureAwait(false);
                    Image objMugshot = await objExistingCache.GetMugshotAsync(token).ConfigureAwait(false);
                    Interlocked.Exchange(ref _imgMugshot, objMugshot.Clone() as Image)?.Dispose();
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
        public SafeAsyncEventHandler OnMyDoubleClick
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyDoubleClick;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public SafeAsyncEventHandler OnMyContextMenuDeleteClick
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyContextMenuDeleteClick;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public SafeAsyncEventHandler<TreeViewEventArgs> OnMyAfterSelect
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyAfterSelect;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public SafeAsyncEventHandler<ValueTuple<KeyEventArgs, TreeNode>> OnMyKeyDown
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _onMyKeyDown;
            }
        }

        public async Task OnDefaultDoubleClick(object sender, EventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strFilePath = string.Empty;
            Character objOpenCharacter;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strFileName = await GetFileNameAsync(token).ConfigureAwait(false);
                objOpenCharacter = await Program.OpenCharacters
                    .FirstOrDefaultAsync(x => string.Equals(x.FileName, strFileName, StringComparison.Ordinal), token)
                    .ConfigureAwait(false);
                if (objOpenCharacter == null)
                {
                    strFilePath = await GetFilePathAsync(token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(strFilePath))
            {
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program
                               .CreateAndShowProgressBarAsync(
                                   strFilePath, Character.NumLoadingSections, token)
                               .ConfigureAwait(false))
                    objOpenCharacter = await Program
                        .LoadCharacterAsync(strFilePath, frmLoadingBar: frmLoadingBar.MyForm, token: token)
                        .ConfigureAwait(false);
            }

            if (!await Program.SwitchToOpenCharacter(objOpenCharacter, token).ConfigureAwait(false))
                await Program.OpenCharacter(objOpenCharacter, token: token).ConfigureAwait(false);
        }

        public async Task OnDefaultContextMenuDeleteClick(object sender, EventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sender is TreeNode t)
            {
                switch (t.Parent.Tag?.ToString())
                {
                    case "Recent":
                        await GlobalSettings.MostRecentlyUsedCharacters.RemoveAsync(await GetFilePathAsync(token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        break;

                    case "Favorite":
                        await GlobalSettings.FavoriteCharacters.RemoveAsync(await GetFilePathAsync(token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
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
                            : await LoadXPathDocumentAsync().ConfigureAwait(false);

                        XPathDocument LoadXPathDocument()
                        {
                            if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromFilePatient(strFile, token: token);
                            if (strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromLzmaCompressedFilePatient(strFile, token: token);
                            Utils.BreakIfDebug();
                            throw new InvalidOperationException();
                        }

                        Task<XPathDocument> LoadXPathDocumentAsync()
                        {
                            if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromFilePatientAsync(strFile, token: token);
                            if (strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                                return XPathDocumentExtensions.LoadStandardFromLzmaCompressedFilePatientAsync(
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
                            using (Image imgMugshot = strMugshotBase64.ToImage(token))
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                imgNewMugshot = imgMugshot.GetCompressedImage(token: token);
                        }
                        else
                        {
                            using (Image imgMugshot
                                   = await strMugshotBase64.ToImageAsync(token: token).ConfigureAwait(false))
                                imgNewMugshot = await imgMugshot.GetCompressedImageAsync(token: token)
                                                                .ConfigureAwait(false);
                        }
                        Image objOldMugshot = Interlocked.Exchange(ref _imgMugshot, imgNewMugshot);
                        if (objOldMugshot != null && !ReferenceEquals(objOldMugshot, imgNewMugshot))
                            objOldMugshot.Dispose();
                    }
                }
                else
                {
                    _strDescription = string.Empty;
                    _strBuildMethod = string.Empty;
                    _strBackground = string.Empty;
                    _strCharacterNotes = string.Empty;
                    _strGameNotes = string.Empty;
                    _strConcept = string.Empty;
                    _strKarma = string.Empty;
                    _strMetatype = string.Empty;
                    _strMetavariant = string.Empty;
                    _strPlayerName = string.Empty;
                    _strCharacterName = string.Empty;
                    _strCharacterAlias = string.Empty;
                    _intCreated = 0;
                    _strEssence = string.Empty;
                    _strSettingsFile = string.Empty;
                    Interlocked.Exchange(ref _imgMugshot, null)?.Dispose();
                }
                _strErrorText = strErrorText;
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
                    string strFilePath = FilePath;
                    if (Program.MainForm.OpenCharacterEditorForms?.Any(
                            x => !x.CharacterObject.IsDisposed && string.Equals(x.CharacterObject.FileName, strFilePath,
                                StringComparison.Ordinal)) == true)
                        strMarker += '*';
                    if (Program.MainForm.OpenCharacterSheetViewers?.Any(
                            x => x.CharacterObjects.Any(y =>
                                !y.IsDisposed && string.Equals(y.FileName, strFilePath,
                                    StringComparison.Ordinal))) == true)
                        strMarker += '^';
                    if (Program.MainForm.OpenCharacterExportForms?.Any(
                            x => !x.CharacterObject.IsDisposed && string.Equals(x.CharacterObject.FileName, strFilePath,
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
                string strErrorText = await GetErrorTextAsync(token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strErrorText))
                {
                    strReturn = Path.GetFileNameWithoutExtension(await GetFileNameAsync(token).ConfigureAwait(false))
                        + strSpace + '(' + await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false) + ')';
                }
                else
                {
                    strReturn = await GetCharacterAliasAsync(token).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strReturn))
                    {
                        strReturn = await GetCharacterNameAsync(token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strReturn))
                            strReturn = await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token)
                                .ConfigureAwait(false);
                    }

                    string strBuildMethod = await LanguageManager.GetStringAsync("String_" + await GetBuildMethodAsync(token).ConfigureAwait(false), false, token)
                        .ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strBuildMethod))
                        strBuildMethod = await LanguageManager.GetStringAsync("String_Unknown", token: token)
                            .ConfigureAwait(false);
                    strReturn += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                                 + await LanguageManager
                                     .GetStringAsync(await GetCreatedAsync(token).ConfigureAwait(false)
                                        ? "Title_CareerMode"
                                        : "Title_CreateMode", token: token)
                                     .ConfigureAwait(false) + ')';
                }

                if (blnAddMarkerIfOpen && Program.MainForm != null)
                {
                    string strMarker = string.Empty;
                    string strFilePath = await GetFilePathAsync(token).ConfigureAwait(false);
                    ThreadSafeObservableCollection<CharacterShared> lstToProcess1
                        = Program.MainForm.OpenCharacterEditorForms;
                    if (lstToProcess1 != null && await lstToProcess1
                            .AnyAsync(
                                async x => !x.CharacterObject.IsDisposed &&
                                           string.Equals(
                                               await x.CharacterObject.GetFileNameAsync(token).ConfigureAwait(false),
                                               strFilePath, StringComparison.Ordinal), token)
                            .ConfigureAwait(false))
                        strMarker += '*';
                    ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess2
                        = Program.MainForm.OpenCharacterSheetViewers;
                    if (lstToProcess2 != null && await lstToProcess2
                            .AnyAsync(
                                x => x.CharacterObjects.AnyAsync(
                                    async y => !y.IsDisposed && string.Equals(
                                        await y.GetFileNameAsync(token).ConfigureAwait(false), strFilePath,
                                        StringComparison.Ordinal), token), token).ConfigureAwait(false))
                        strMarker += '^';
                    ThreadSafeObservableCollection<ExportCharacter> lstToProcess3
                        = Program.MainForm.OpenCharacterExportForms;
                    if (lstToProcess3 != null && await lstToProcess3
                            .AnyAsync(
                                async x => !x.CharacterObject.IsDisposed &&
                                           string.Equals(
                                               await x.CharacterObject.GetFileNameAsync(token).ConfigureAwait(false),
                                               strFilePath, StringComparison.Ordinal), token)
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

        public async Task OnDefaultKeyDown(object sender, ValueTuple<KeyEventArgs, TreeNode> args, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (args.Item1.KeyCode == Keys.Delete)
            {
                switch (args.Item2.Parent.Tag.ToString())
                {
                    case "Recent":
                        await GlobalSettings.MostRecentlyUsedCharacters.RemoveAsync(await GetFilePathAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                        break;

                    case "Favorite":
                        await GlobalSettings.FavoriteCharacters.RemoveAsync(await GetFilePathAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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
