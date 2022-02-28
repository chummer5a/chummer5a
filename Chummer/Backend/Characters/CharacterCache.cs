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
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ErrorText { get; set; }
        public string Description { get; set; }
        public string Background { get; set; }
        public string GameNotes { get; set; }
        public string CharacterNotes { get; set; }
        public string Concept { get; set; }
        public string Karma { get; set; }
        public string Metatype { get; set; }
        public string Metavariant { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string CharacterAlias { get; set; }
        public string BuildMethod { get; set; }
        public string Essence { get; set; }

        private bool _blnIsLoadMethodRunning;

        public bool IsLoadMethodRunning
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return _blnIsLoadMethodRunning;
            }
            set
            {
                using (new EnterWriteLock(LockObject))
                    _blnIsLoadMethodRunning = value;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Image Mugshot { get; private set; }

        public bool Created { get; set; }

        public string SettingsFile { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public LockingDictionary<string, object> MyPluginDataDic { get; } = new LockingDictionary<string, object>();

        public Task<string> DownLoadRunning { get; set; }

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
        public static async ValueTask<CharacterCache> CreateFromFileAsync(string strFile)
        {
            CharacterCache objReturn = new CharacterCache();
            await objReturn.LoadFromFileAsync(strFile);
            return objReturn;
        }

        public void CopyFrom(CharacterCache objExistingCache)
        {
            Background = objExistingCache.Background;
            BuildMethod = objExistingCache.BuildMethod;
            CharacterAlias = objExistingCache.CharacterAlias;
            CharacterName = objExistingCache.CharacterName;
            CharacterNotes = objExistingCache.CharacterNotes;
            Concept = objExistingCache.Concept;
            Created = objExistingCache.Created;
            Description = objExistingCache.Description;
            Essence = objExistingCache.Essence;
            GameNotes = objExistingCache.GameNotes;
            Karma = objExistingCache.Karma;
            FileName = objExistingCache.FileName;
            Metatype = objExistingCache.Metatype;
            Metavariant = objExistingCache.Metavariant;
            PlayerName = objExistingCache.PlayerName;
            SettingsFile = objExistingCache.SettingsFile;

            Image imgNewMugshot = objExistingCache.Mugshot.Clone() as Image;
            Mugshot?.Dispose();
            Mugshot = imgNewMugshot;
        }

        private void SetDefaultEventHandlers()
        {
            OnMyDoubleClick += OnDefaultDoubleClick;
            OnMyKeyDown += OnDefaultKeyDown;
            OnMyContextMenuDeleteClick += OnDefaultContextMenuDeleteClick;
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyDoubleClick { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyContextMenuDeleteClick { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<TreeViewEventArgs> OnMyAfterSelect { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<Tuple<KeyEventArgs, TreeNode>> OnMyKeyDown { get; set; }

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
            while (IsLoadMethodRunning)
            {
                if (blnSync)
                    Utils.SafeSleep();
                else
                    await Utils.SafeSleepAsync();
            }

            IsLoadMethodRunning = true;
            try
            {
                DownLoadRunning = null;
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

                        xmlSourceNode = xmlDoc.CreateNavigator().SelectSingleNodeAndCacheExpression("/character");
                    }
                    catch (Exception ex)
                    {
                        xmlSourceNode = null;
                        strErrorText = ex.ToString();
                    }
                }

                if (xmlSourceNode != null)
                {
                    Description = xmlSourceNode.SelectSingleNodeAndCacheExpression("description")?.Value;
                    BuildMethod = xmlSourceNode.SelectSingleNodeAndCacheExpression("buildmethod")?.Value;
                    Background = xmlSourceNode.SelectSingleNodeAndCacheExpression("background")?.Value;
                    CharacterNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("notes")?.Value;
                    GameNotes = xmlSourceNode.SelectSingleNodeAndCacheExpression("gamenotes")?.Value;
                    Concept = xmlSourceNode.SelectSingleNodeAndCacheExpression("concept")?.Value;
                    Karma = xmlSourceNode.SelectSingleNodeAndCacheExpression("totalkarma")?.Value;
                    Metatype = xmlSourceNode.SelectSingleNodeAndCacheExpression("metatype")?.Value;
                    Metavariant = xmlSourceNode.SelectSingleNodeAndCacheExpression("metavariant")?.Value;
                    PlayerName = xmlSourceNode.SelectSingleNodeAndCacheExpression("playername")?.Value;
                    CharacterName = xmlSourceNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                    CharacterAlias = xmlSourceNode.SelectSingleNodeAndCacheExpression("alias")?.Value;
                    Created = xmlSourceNode.SelectSingleNodeAndCacheExpression("created")?.Value == bool.TrueString;
                    Essence = xmlSourceNode.SelectSingleNodeAndCacheExpression("totaless")?.Value;
                    string strSettings = xmlSourceNode.SelectSingleNodeAndCacheExpression("settings")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSettings))
                    {
                        if (SettingsManager.LoadedCharacterSettings.TryGetValue(
                            strSettings, out CharacterSettings objSettings))
                            SettingsFile = objSettings.DisplayName;
                        else
                        {
                            string strTemp = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("MessageTitle_FileNotFound") +
                                  // ReSharper disable once MethodHasAsyncOverload
                                  LanguageManager.GetString("String_Space")
                                : await LanguageManager.GetStringAsync("MessageTitle_FileNotFound") +
                                  await LanguageManager.GetStringAsync("String_Space");
                            SettingsFile = strTemp + '[' + strSettings + ']';
                        }
                    }
                    else
                        SettingsFile = string.Empty;
                    string strMugshotBase64 = xmlSourceNode.SelectSingleNodeAndCacheExpression("mugshot")?.Value ?? string.Empty;
                    if (string.IsNullOrEmpty(strMugshotBase64))
                    {
                        XPathNavigator xmlMainMugshotIndex = xmlSourceNode.SelectSingleNodeAndCacheExpression("mainmugshotindex");
                        if (xmlMainMugshotIndex != null &&
                            int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) &&
                            intMainMugshotIndex >= 0)
                        {
                            XPathNodeIterator xmlMugshotList = xmlSourceNode.SelectAndCacheExpression("mugshots/mugshot");
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
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            using (Image imgMugshot = strMugshotBase64.ToImage())
                                // ReSharper disable once MethodHasAsyncOverload
                                Mugshot = imgMugshot.GetCompressedImage();
                        }
                        else
                        {
                            using (Image imgMugshot = await strMugshotBase64.ToImageAsync())
                                Mugshot = await imgMugshot.GetCompressedImageAsync();
                        }
                    }
                }
                else
                {
                    ErrorText = strErrorText;
                }

                FilePath = strFile;
                if (!string.IsNullOrEmpty(strFile))
                {
                    int last = strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    if (strFile.Length > last)
                        FileName = strFile.Substring(last);
                }

                return string.IsNullOrEmpty(strErrorText);
            }
            finally
            {
                IsLoadMethodRunning = false;
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

        /// <inheritdoc />
        public void Dispose()
        {
            Mugshot?.Dispose();
            DownLoadRunning?.Dispose();
            MyPluginDataDic.Dispose();
            LockObject.Dispose();
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
