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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
        private bool _blnCreated;
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
                {
                    if (_strFilePath == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strFilePath = value;
                }
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
                {
                    if (_strFileName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strFileName = value;
                }
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
                {
                    if (_strErrorText == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strErrorText = value;
                }
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
                {
                    if (_strDescription == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strDescription = value;
                }
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
                {
                    if (_strBackground == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strBackground = value;
                }
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
                {
                    if (_strGameNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strGameNotes = value;
                }
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
                {
                    if (_strCharacterNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strCharacterNotes = value;
                }
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
                {
                    if (_strConcept == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strConcept = value;
                }
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
                {
                    if (_strKarma == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strKarma = value;
                }
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
                {
                    if (_strMetatype == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strMetatype = value;
                }
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
                {
                    if (_strMetavariant == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strMetavariant = value;
                }
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
                {
                    if (_strPlayerName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strPlayerName = value;
                }
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
                {
                    if (_strCharacterName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strCharacterName = value;
                }
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
                {
                    if (_strCharacterAlias == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strCharacterAlias = value;
                }
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
                {
                    if (_strBuildMethod == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strBuildMethod = value;
                }
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
                {
                    if (_strEssence == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strEssence = value;
                }
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
                Image imgOldValue;
                using (EnterReadLock.Enter(LockObject))
                {
                    imgOldValue = _imgMugshot;
                    if (imgOldValue == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _imgMugshot = value;
                }
                imgOldValue?.Dispose();
            }
        }

        public bool Created
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnCreated;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnCreated == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnCreated = value;
                }
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
                {
                    if (_strSettingsFile == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strSettingsFile = value;
                }
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
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_tskRunningDownloadTask == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _tskRunningDownloadTask = value;
                }
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
        /// <param name="strFile"></param>
        public CharacterCache(string strFile) : this()
        {
            LoadFromFile(strFile);
        }

        /// <summary>
        /// Syntactic sugar to call LoadFromFile() asynchronously immediately after the constructor.
        /// </summary>
        /// <param name="strFile"></param>
        public static Task<CharacterCache> CreateFromFileAsync(string strFile)
        {
            CharacterCache objReturn = new CharacterCache();
            return objReturn.LoadFromFileAsync(strFile).ContinueWith(x => objReturn);
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
                _blnCreated = objExistingCache.Created;
                _strDescription = objExistingCache.Description;
                _strEssence = objExistingCache.Essence;
                _strGameNotes = objExistingCache.GameNotes;
                _strKarma = objExistingCache.Karma;
                _strFileName = objExistingCache.FileName;
                _strMetatype = objExistingCache.Metatype;
                _strMetavariant = objExistingCache.Metavariant;
                _strPlayerName = objExistingCache.PlayerName;
                _strSettingsFile = objExistingCache.SettingsFile;
                _imgMugshot?.Dispose();
                _imgMugshot = objExistingCache.Mugshot.Clone() as Image;
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
                {
                    if (_onMyDoubleClick == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _onMyDoubleClick = value;
                }
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
                {
                    if (_onMyContextMenuDeleteClick == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _onMyContextMenuDeleteClick = value;
                }
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
                {
                    if (_onMyAfterSelect == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _onMyAfterSelect = value;
                }
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
                {
                    if (_onMyKeyDown == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _onMyKeyDown = value;
                }
            }
        }

        public async void OnDefaultDoubleClick(object sender, EventArgs e)
        {
            Character objOpenCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == FileName)
                                         ?? await Program.LoadCharacterAsync(FilePath);
            if (!Program.SwitchToOpenCharacter(objOpenCharacter))
                await Program.OpenCharacter(objOpenCharacter);
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
            return LoadFromFileCoreAsync(true, strFile).GetAwaiter().GetResult();
        }

        public Task<bool> LoadFromFileAsync(string strFile)
        {
            return LoadFromFileCoreAsync(false, strFile);
        }

        private async Task<bool> LoadFromFileCoreAsync(bool blnSync, string strFile)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
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
                        ? LanguageManager.GetString("MessageTitle_FileNotFound")
                        : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound");
                }
                else
                {
                    // If we run into any problems loading the character cache, fail out early.
                    try
                    {
                        XPathDocument xmlDoc = blnSync ? LoadXPathDocument() : await Task.Run(LoadXPathDocument);

                        XPathDocument LoadXPathDocument()
                        {
                            using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                            {
                                using (XmlReader objXmlReader =
                                    XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                                {
                                    return new XPathDocument(objXmlReader);
                                }
                            }
                        }

                        xmlSourceNode = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? xmlDoc.CreateNavigator().SelectSingleNodeAndCacheExpression("/character")
                            : await xmlDoc.CreateNavigator().SelectSingleNodeAndCacheExpressionAsync("/character");
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
                        // ReSharper disable MethodHasAsyncOverload
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
                        _blnCreated = xmlSourceNode.SelectSingleNodeAndCacheExpression("created")?.Value
                                      == bool.TrueString;
                        _strEssence = xmlSourceNode.SelectSingleNodeAndCacheExpression("totaless")?.Value;
                        // ReSharper restore MethodHasAsyncOverload
                    }
                    else
                    {
                        _strDescription = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("description"))?.Value;
                        _strBuildMethod = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("buildmethod"))?.Value;
                        _strBackground = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("background"))?.Value;
                        _strCharacterNotes = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("notes"))?.Value;
                        _strGameNotes = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("gamenotes"))?.Value;
                        _strConcept = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("concept"))?.Value;
                        _strKarma = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("totalkarma"))?.Value;
                        _strMetatype = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("metatype"))?.Value;
                        _strMetavariant = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("metavariant"))?.Value;
                        _strPlayerName = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("playername"))?.Value;
                        _strCharacterName = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value;
                        _strCharacterAlias = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("alias"))?.Value;
                        _blnCreated = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("created"))?.Value == bool.TrueString;
                        _strEssence = (await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("totaless"))?.Value;
                    }

                    string strSettings
                        = (blnSync
                              // ReSharper disable once MethodHasAsyncOverload
                              ? xmlSourceNode.SelectSingleNodeAndCacheExpression("settings")
                              : await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("settings"))?.Value
                          ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSettings))
                    {
                        (bool blnSuccess, CharacterSettings objSettings)
                            = await SettingsManager.LoadedCharacterSettingsAsModifiable.TryGetValueAsync(strSettings);
                        if (blnSuccess)
                            _strSettingsFile = objSettings.DisplayName;
                        else
                        {
                            string strTemp = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("MessageTitle_FileNotFound") +
                                  // ReSharper disable once MethodHasAsyncOverload
                                  LanguageManager.GetString("String_Space")
                                : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound") +
                                  await LanguageManager.GetStringAsync("String_Space");
                            _strSettingsFile = strTemp + '[' + strSettings + ']';
                        }
                    }
                    else
                        _strSettingsFile = string.Empty;

                    string strMugshotBase64
                        = (blnSync
                              // ReSharper disable once MethodHasAsyncOverload
                              ? xmlSourceNode.SelectSingleNodeAndCacheExpression("mugshot")
                              : await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("mugshot"))?.Value
                          ?? string.Empty;
                    if (string.IsNullOrEmpty(strMugshotBase64))
                    {
                        XPathNavigator xmlMainMugshotIndex = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? xmlSourceNode.SelectSingleNodeAndCacheExpression("mainmugshotindex")
                            : await xmlSourceNode.SelectSingleNodeAndCacheExpressionAsync("mainmugshotindex");
                        if (xmlMainMugshotIndex != null &&
                            int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) &&
                            intMainMugshotIndex >= 0)
                        {
                            XPathNodeIterator xmlMugshotList = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? xmlSourceNode.SelectAndCacheExpression("mugshots/mugshot")
                                : await xmlSourceNode.SelectAndCacheExpressionAsync("mugshots/mugshot");
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
                        _imgMugshot?.Dispose();
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            using (Image imgMugshot = strMugshotBase64.ToImage())
                                // ReSharper disable once MethodHasAsyncOverload
                                _imgMugshot = imgMugshot.GetCompressedImage();
                        }
                        else
                        {
                            using (Image imgMugshot = await strMugshotBase64.ToImageAsync())
                                _imgMugshot = await imgMugshot.GetCompressedImageAsync();
                        }
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
                await objLocker.DisposeAsync();
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
            if (blnAddMarkerIfOpen && Program.MainForm?.OpenCharacterForms.Any(x => x.CharacterObject.FileName == FilePath) == true)
                strReturn = '*' + strSpace + strReturn;
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

        private bool _blnIsDisposed;

        public bool IsDisposed => _blnIsDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_blnIsDisposed)
                return;
            using (LockObject.EnterWriteLock())
            {
                _blnIsDisposed = true;
                _imgMugshot?.Dispose();
                _tskRunningDownloadTask?.Dispose();
                _dicMyPluginData.Dispose();
            }
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_blnIsDisposed)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _blnIsDisposed = true;
                _imgMugshot?.Dispose();
                _tskRunningDownloadTask?.Dispose();
                await _dicMyPluginData.DisposeAsync();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await LockObject.DisposeAsync();
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
