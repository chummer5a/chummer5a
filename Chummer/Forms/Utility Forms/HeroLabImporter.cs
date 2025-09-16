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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Enums;

namespace Chummer
{
    public partial class HeroLabImporter : Form
    {
        private readonly ThreadSafeList<HeroLabCharacterCache> _lstCharacterCache = new ThreadSafeList<HeroLabCharacterCache>(2);
        private readonly ConcurrentDictionary<string, Bitmap> _dicImages = new ConcurrentDictionary<string, Bitmap>();

        public HeroLabImporter()
        {
            Disposed += (sender, args) =>
            {
                _lstCharacterCache.Dispose();
                List<Bitmap> lstImages = _dicImages.GetValuesToListSafe();
                _dicImages.Clear();
                foreach (Bitmap objImage in lstImages)
                    objImage.Dispose();
                dlgOpenFile?.Dispose();
            };
            InitializeComponent();
            tabCharacterText.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
        }

        private async void HeroLabImporter_Load(object sender, EventArgs e)
        {
            dlgOpenFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_HeroLab").ConfigureAwait(false) + '|'
                + await LanguageManager.GetStringAsync("DialogFilter_All").ConfigureAwait(false);
        }

        private async void cmdSelectFile_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to possess.
            if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x)).ConfigureAwait(false) != DialogResult.OK)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                string strSelectedFile = dlgOpenFile.FileName;
                TreeNode objNode = await CacheCharacters(strSelectedFile).ConfigureAwait(false);
                if (objNode != null)
                {
                    await treCharacterList.DoThreadSafeAsync(x =>
                    {
                        x.Nodes.Clear();
                        x.Nodes.Add(objNode);
                        x.SelectedNode = objNode.Nodes.Count > 0 ? objNode.Nodes[0] : objNode;
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="token"></param>
        private async Task<TreeNode> CacheCharacters(string strFile, CancellationToken token = default)
        {
            if (!File.Exists(strFile))
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token).ConfigureAwait(false), strFile), token: token).ConfigureAwait(false);
                return null;
            }

            ConcurrentBag<XPathNavigator> lstCharacterXmlStatblocks = new ConcurrentBag<XPathNavigator>();
            try
            {
                using (ZipArchive zipArchive
                       = ZipFile.Open(strFile, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                {
                    // NOTE: Cannot parallelize because ZipFile.Open creates one handle on the entire zip file that gets messed up if we try to get it to read multiple files at once
                    foreach (ZipArchiveEntry objEntry in zipArchive.Entries)
                    {
                        string strEntryFullName = objEntry.FullName;
                        if (strEntryFullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                            && strEntryFullName.StartsWith("statblocks_xml", StringComparison.Ordinal))
                        {
                            // If we run into any problems loading the character cache, fail out early.
                            try
                            {
                                await TaskExtensions.RunWithoutEC(() =>
                                {
                                    XPathDocument xmlSourceDoc;
                                    using (Stream objStream = objEntry.Open())
                                    {
                                        token.ThrowIfCancellationRequested();
                                        using (StreamReader sr = new StreamReader(objStream, true))
                                        {
                                            token.ThrowIfCancellationRequested();
                                            using (XmlReader objXmlReader
                                                   = XmlReader.Create(sr, GlobalSettings.SafeXmlReaderSettings))
                                            {
                                                token.ThrowIfCancellationRequested();
                                                xmlSourceDoc = new XPathDocument(objXmlReader);
                                            }
                                        }
                                    }

                                    XPathNavigator objToAdd = xmlSourceDoc.CreateNavigator();
                                    lstCharacterXmlStatblocks.Add(objToAdd);
                                }, token).ConfigureAwait(false);
                            }
                            // If we run into any problems loading the character cache, fail out early.
                            catch (IOException)
                            {
                                Utils.BreakIfDebug();
                            }
                            catch (XmlException)
                            {
                                Utils.BreakIfDebug();
                            }
                        }
                        else if (strEntryFullName.StartsWith("images", StringComparison.Ordinal)
                                 && strEntryFullName.Contains('.'))
                        {
                            using (Stream objStream = objEntry.Open())
                            {
                                token.ThrowIfCancellationRequested();
                                using (Bitmap bmpMugshot = new Bitmap(objStream, true))
                                {
                                    token.ThrowIfCancellationRequested();
                                    Bitmap bmpNewMugshot = bmpMugshot.PixelFormat == PixelFormat.Format32bppPArgb
                                        ? bmpMugshot.Clone() as Bitmap // Clone makes sure file handle is closed
                                        : bmpMugshot.ConvertPixelFormat(PixelFormat.Format32bppPArgb);
                                    token.ThrowIfCancellationRequested();
                                    string strKey = Path.GetFileName(strEntryFullName);
                                    _dicImages.AddOrUpdate(strKey, x => bmpNewMugshot, (x, y) =>
                                    {
                                        y.Dispose();
                                        return bmpNewMugshot;
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token)
                            .ConfigureAwait(false),
                        strFile), token: token).ConfigureAwait(false);
                return null;
            }
            catch (NotSupportedException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed", token: token)
                            .ConfigureAwait(false),
                        strFile), token: token).ConfigureAwait(false);
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
                return null;
            }

            string strFileText
                = await strFile
                        .CheapReplaceAsync(Utils.GetStartupPath, () => '<' + Application.ProductName + '>',
                                           token: token).ConfigureAwait(false);
            TreeNode nodRootNode = new TreeNode
            {
                Text = strFileText,
                ToolTipText = strFileText
            };

            XPathNavigator xmlMetatypesDocument
                = await XmlManager.LoadXPathAsync("metatypes.xml", token: token).ConfigureAwait(false);
            foreach (XPathNavigator xmlCharacterDocument in lstCharacterXmlStatblocks)
            {
                XPathNavigator xmlBaseCharacterNode
                    = xmlCharacterDocument
                            .SelectSingleNodeAndCacheExpression("/document/public/character", token);
                if (xmlBaseCharacterNode != null)
                {
                    HeroLabCharacterCache objCache = new HeroLabCharacterCache
                    {
                        PlayerName = xmlBaseCharacterNode
                            .SelectSingleNodeAndCacheExpression("@playername", token)?.Value ?? string.Empty
                    };
                    string strNameString
                        = xmlBaseCharacterNode.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;
                    objCache.CharacterId = strNameString;
                    if (!string.IsNullOrEmpty(strNameString))
                    {
                        int intAsIndex = strNameString.IndexOf(" as ", StringComparison.Ordinal);
                        if (intAsIndex != -1)
                        {
                            objCache.CharacterName = strNameString.Substring(0, intAsIndex);
                            objCache.CharacterAlias
                                = strNameString.Substring(intAsIndex).TrimStart(" as ").Trim('\'');
                        }
                        else
                        {
                            objCache.CharacterName = strNameString;
                        }
                    }

                    string strRaceString = xmlBaseCharacterNode
                        .SelectSingleNodeAndCacheExpression("race/@name", token)?.Value;
                    if (strRaceString == "Metasapient")
                        strRaceString = "A.I.";
                    if (!string.IsNullOrEmpty(strRaceString))
                    {
                        foreach (XPathNavigator xmlMetatype in xmlMetatypesDocument.SelectAndCacheExpression(
                                     "/chummer/metatypes/metatype", token))
                        {
                            string strMetatypeName
                                = xmlMetatype.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                            if (strMetatypeName == strRaceString)
                            {
                                objCache.Metatype = strMetatypeName;
                                objCache.Metavariant = "None";
                                break;
                            }

                            foreach (XPathNavigator xmlMetavariant in
                                     xmlMetatype
                                           .SelectAndCacheExpression("metavariants/metavariant", token: token))
                            {
                                string strMetavariantName
                                    = xmlMetavariant.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                                if (strMetavariantName == strRaceString)
                                {
                                    objCache.Metatype = strMetatypeName;
                                    objCache.Metavariant = strMetavariantName;
                                    break;
                                }
                            }
                        }
                    }

                    objCache.Description = xmlBaseCharacterNode
                        .SelectSingleNodeAndCacheExpression(
                            "personal/description", token)?.Value;
                    objCache.Karma
                        = xmlBaseCharacterNode.SelectSingleNodeAndCacheExpression("karma/@total", token)?.Value ?? "0";
                    objCache.Essence = xmlBaseCharacterNode
                        .SelectSingleNodeAndCacheExpression(
                            "attributes/attribute[@name = \"Essence\"]/@text", token)?.Value;
                    objCache.BuildMethod
                        = xmlBaseCharacterNode
                            .SelectSingleNodeAndCacheExpression("creation/bp/@total", token)?.ValueAsInt <= 100
                            ? CharacterBuildMethod.Priority
                            : CharacterBuildMethod.Karma;

                    string strSettingsSummary =
                        xmlBaseCharacterNode.SelectSingleNodeAndCacheExpression("settings/@summary", token)?.Value;
                    if (!string.IsNullOrEmpty(strSettingsSummary))
                    {
                        int intSemicolonIndex;
                        bool blnDoFullHouse = false;
                        int intSourcebooksIndex
                            = strSettingsSummary.IndexOf("Core Rulebooks:", StringComparison.OrdinalIgnoreCase);
                        if (intSourcebooksIndex != -1)
                        {
                            intSemicolonIndex = strSettingsSummary.IndexOf(';', intSourcebooksIndex);
                            if (intSourcebooksIndex + 16 < intSemicolonIndex)
                            {
                                blnDoFullHouse
                                    = true; // We probably have multiple books enabled, so use Full House instead
                            }
                        }

                        string strHeroLabSettingsName = "Standard";

                        int intCharCreationSystemsIndex =
                            strSettingsSummary.IndexOf("Character Creation Systems:",
                                                       StringComparison.OrdinalIgnoreCase);
                        if (intCharCreationSystemsIndex != -1)
                        {
                            intSemicolonIndex = strSettingsSummary.IndexOf(';', intCharCreationSystemsIndex);
                            if (intCharCreationSystemsIndex + 28 <= intSemicolonIndex)
                            {
                                strHeroLabSettingsName = strSettingsSummary.Substring(
                                                                               intCharCreationSystemsIndex + 28,
                                                                               strSettingsSummary.IndexOf(
                                                                                   ';', intCharCreationSystemsIndex)
                                                                               - 28 - intCharCreationSystemsIndex)
                                                                           .Trim();
                                if (strHeroLabSettingsName == "Established Runners")
                                    strHeroLabSettingsName = "Standard";
                            }
                        }

                        if (strHeroLabSettingsName == "Standard")
                        {
                            if (blnDoFullHouse)
                            {
                                strHeroLabSettingsName = objCache.BuildMethod == CharacterBuildMethod.Karma
                                    ? "Full House (Point Buy)"
                                    : "Full House";
                            }
                            else if (objCache.BuildMethod == CharacterBuildMethod.Karma)
                                strHeroLabSettingsName = "Point Buy";
                        }

                        objCache.SettingsName = strHeroLabSettingsName;
                    }

                    objCache.Created = objCache.Karma != "0";
                    if (!objCache.Created)
                    {
                        XPathNodeIterator xmlJournalEntries
                            = xmlBaseCharacterNode.SelectAndCacheExpression("journals/journal", token: token);
                        if (xmlJournalEntries?.Count > 1
                            || (xmlJournalEntries?.Count == 1 &&
                                xmlJournalEntries.Current != null
                                && xmlJournalEntries.Current
                                    .SelectSingleNodeAndCacheExpression(
                                        "@name", token)?.Value
                                != "Title"))
                        {
                            objCache.Created = true;
                        }
                    }

                    string strImageString = xmlBaseCharacterNode
                        .SelectSingleNodeAndCacheExpression(
                            "images/image/@filename", token)?.Value;
                    if (!string.IsNullOrEmpty(strImageString) && _dicImages.TryGetValue(strImageString, out Bitmap objTemp))
                    {
                        objCache.Mugshot = objTemp;
                    }

                    objCache.FilePath = strFile;
                    TreeNode objNode = new TreeNode
                    {
                        Text = await CalculatedName(objCache, token).ConfigureAwait(false),
                        ToolTipText = await strFile.CheapReplaceAsync(Utils.GetStartupPath,
                                                                      () => '<' + Application.ProductName + '>',
                                                                      token: token).ConfigureAwait(false)
                    };
                    nodRootNode.Nodes.Add(objNode);

                    await _lstCharacterCache.AddAsync(objCache, token).ConfigureAwait(false);
                    objNode.Tag = await _lstCharacterCache.IndexOfAsync(objCache, token).ConfigureAwait(false);
                }
            }

            nodRootNode.Expand();
            return nodRootNode;
        }

        #region Classes

        /// <summary>
        /// Caches a subset of a full character's properties for loading purposes.
        /// </summary>
        private sealed class HeroLabCharacterCache
        {
            internal string FilePath { get; set; }
            internal string Description { get; set; }
            internal string Karma { get; set; }
            internal string Metatype { get; set; }
            internal string Metavariant { get; set; }
            internal string PlayerName { get; set; }
            internal string CharacterId { get; set; }
            internal string CharacterName { get; set; }
            internal string CharacterAlias { get; set; }
            internal CharacterBuildMethod BuildMethod { get; set; }
            internal string SettingsName { get; set; }
            internal string Essence { get; set; }
            internal Image Mugshot { get; set; }
            internal bool Created { get; set; }
        }

        #endregion Classes

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="objCache"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<string> CalculatedName(HeroLabCharacterCache objCache, CancellationToken token = default)
        {
            string strName = objCache.CharacterAlias;
            if (string.IsNullOrEmpty(strName))
            {
                strName = objCache.CharacterName;
                if (string.IsNullOrEmpty(strName))
                    strName = await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token).ConfigureAwait(false);
            }
            string strBuildMethod = await LanguageManager.GetStringAsync("String_" + objCache.BuildMethod, false, token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strBuildMethod))
                strBuildMethod = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            strName += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                       + await LanguageManager.GetStringAsync(objCache.Created ? "Title_CareerMode" : "Title_CreateMode", token: token).ConfigureAwait(false) + ')';
            return strName;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        /// <param name="token"></param>
        private async Task UpdateCharacter(HeroLabCharacterCache objCache, CancellationToken token = default)
        {
            if (objCache != null)
            {
                await txtCharacterBio.DoThreadSafeAsync(x => x.Text = objCache.Description, token: token).ConfigureAwait(false);

                string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);

                await lblCharacterName.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.CharacterName;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblCharacterNameLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                await lblCharacterAlias.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.CharacterAlias;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblCharacterAliasLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                await lblPlayerName.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.PlayerName;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblPlayerNameLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                string strNone = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                await lblCareerKarma.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.Karma;
                    if (string.IsNullOrEmpty(x.Text) || x.Text == 0.ToString(GlobalSettings.CultureInfo))
                        x.Text = strNone;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblCareerKarmaLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                await lblEssence.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.Essence;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                await picMugshot.DoThreadSafeAsync(x => x.Image = objCache.Mugshot, token: token).ConfigureAwait(false);

                // Populate character information fields.
                XPathNavigator objMetatypeDoc = await XmlManager.LoadXPathAsync("metatypes.xml", token: token).ConfigureAwait(false);
                XPathNavigator objMetatypeNode = objMetatypeDoc.TryGetNodeByNameOrId("/chummer/metatypes/metatype", objCache.Metatype);
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = await XmlManager.LoadXPathAsync("critters.xml", token: token).ConfigureAwait(false);
                    objMetatypeNode = objMetatypeDoc.TryGetNodeByNameOrId("/chummer/metatypes/metatype", objCache.Metatype);
                }

                string strMetatype = objMetatypeNode != null
                    ? objMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                      ?? objCache.Metatype
                    : objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.TryGetNodeByNameOrId("metavariants/metavariant", objCache.Metavariant);

                    strMetatype += " (" + (objMetatypeNode != null
                        ? objMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                          ?? objCache.Metavariant
                        : objCache.Metavariant) + ')';
                }

                await lblMetatype.DoThreadSafeAsync(x =>
                {
                    x.Text = strMetatype;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                }, token: token).ConfigureAwait(false);
                await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);

                await cmdImport.DoThreadSafeAsync(x => x.Enabled = true, token: token).ConfigureAwait(false);
            }
            else
            {
                await txtCharacterBio.DoThreadSafeAsync(x => x.Text = string.Empty, token: token).ConfigureAwait(false);

                await lblCharacterNameLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblCharacterName.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblCharacterAliasLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblCharacterAlias.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblPlayerNameLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblPlayerName.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblMetatype.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblCareerKarmaLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblCareerKarma.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                await lblEssence.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);

                await picMugshot.DoThreadSafeAsync(x => x.Image = null, token: token).ConfigureAwait(false);
                await cmdImport.DoThreadSafeAsync(x => x.Enabled = false, token: token).ConfigureAwait(false);
            }

            await ProcessMugshot(token).ConfigureAwait(false);
        }

        #region Form Methods

        private async void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            HeroLabCharacterCache objCache = null;
            TreeNode objSelectedNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode).ConfigureAwait(false);
            if (objSelectedNode?.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag, GlobalSettings.InvariantCultureInfo);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                    objCache = _lstCharacterCache[intIndex];
            }
            await UpdateCharacter(objCache).ConfigureAwait(false);
            await treCharacterList.DoThreadSafeAsync(x => x.ClearNodeBackground(x.SelectedNode)).ConfigureAwait(false);
        }

        private async void cmdImport_Click(object sender, EventArgs e)
        {
            await DoImport().ConfigureAwait(false);
        }

        private async void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;
            await ProcessMugshot().ConfigureAwait(false);
        }

        private Task ProcessMugshot(CancellationToken token = default)
        {
            return picMugshot.IsNullOrDisposed()
                ? Task.CompletedTask
                : picMugshot.DoThreadSafeAsync(x =>
                {
                    try
                    {
                        x.SizeMode = x.Image != null && x.Height >= x.Image.Height
                                                     && x.Width >= x.Image.Width
                            ? PictureBoxSizeMode.CenterImage
                            : PictureBoxSizeMode.Zoom;
                    }
                    catch (ArgumentException) // No other way to catch when the Image is not null, but is disposed
                    {
                        x.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }, token);
        }

        #endregion Form Methods

        private async Task DoImport(CancellationToken token = default)
        {
            TreeNode objSelectedNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, token).ConfigureAwait(false);
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            int intIndex = Convert.ToInt32(objSelectedNode.Tag, GlobalSettings.InvariantCultureInfo);
            if (intIndex < 0 || intIndex >= _lstCharacterCache.Count)
                return;
            HeroLabCharacterCache objCache = _lstCharacterCache[intIndex];
            if (objCache == null)
                return;
            string strFile = objCache.FilePath;
            string strCharacterId = objCache.CharacterId;
            if (string.IsNullOrEmpty(strFile) || string.IsNullOrEmpty(strCharacterId))
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                bool blnLoaded = false;
                Character objCharacter = new Character();
                try
                {
                    await Program.OpenCharacters.AddAsync(objCharacter, token: token).ConfigureAwait(false);
                    IReadOnlyDictionary<string, CharacterSettings> dicCharacterSettings = await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false);
                    CharacterSettings objHeroLabSettings =
                        dicCharacterSettings.FirstOrDefault(
                            x => x.Value.Name == objCache.SettingsName && x.Value.BuildMethod == objCache.BuildMethod).Value;
                    if (objHeroLabSettings != null)
                    {
                        await objCharacter.SetSettingsKeyAsync(await objHeroLabSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }
                    else
                    {
                        objHeroLabSettings = dicCharacterSettings.FirstOrDefault(
                            x => x.Value.Name.Contains(objCache.SettingsName) && x.Value.BuildMethod == objCache.BuildMethod).Value;
                        if (objHeroLabSettings != null)
                        {
                            await objCharacter.SetSettingsKeyAsync(await objHeroLabSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                        }
                        else
                        {
                            bool blnCacheUsesPriorityTables = objCache.BuildMethod.UsesPriorityTables();
                            if (dicCharacterSettings.TryGetValue(
                                    GlobalSettings.DefaultCharacterSetting, out CharacterSettings objDefaultCharacterSettings) && blnCacheUsesPriorityTables
                                == objDefaultCharacterSettings.BuildMethod.UsesPriorityTables())
                            {
                                await objCharacter.SetSettingsKeyAsync(await objDefaultCharacterSettings.GetDictionaryKeyAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                            }
                            else
                            {
                                CharacterSettings objTempSetting
                                    = dicCharacterSettings.FirstOrDefault(
                                        x => x.Value.BuiltInOption && x.Value.BuildMethod == objCache.BuildMethod).Value;
                                if (objTempSetting != null)
                                    await objCharacter.SetSettingsKeyAsync(await objTempSetting.GetDictionaryKeyAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                                else
                                {
                                    objTempSetting = dicCharacterSettings.FirstOrDefault(
                                        x => x.Value.BuiltInOption && x.Value.BuildMethod.UsesPriorityTables()
                                            == blnCacheUsesPriorityTables).Value;
                                    if (objTempSetting != null)
                                        await objCharacter.SetSettingsKeyAsync(await objTempSetting.GetDictionaryKeyAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                                    else
                                        await objCharacter.SetSettingsKeyAsync(GlobalSettings.DefaultCharacterSetting, token).ConfigureAwait(false);
                                }
                            }
                        }
                    }

                    using (ThreadSafeForm<SelectBuildMethod> frmPickBP
                           = await ThreadSafeForm<SelectBuildMethod>.GetAsync(
                               () => new SelectBuildMethod(objCharacter, true), token).ConfigureAwait(false))
                    {
                        if (await frmPickBP.ShowDialogSafeAsync(this, token).ConfigureAwait(false) != DialogResult.OK)
                            return;
                    }

                    //Timekeeper.Start("load_file");
                    if (!await objCharacter.LoadFromHeroLabFileAsync(strFile, strCharacterId, await objCharacter.GetSettingsKeyAsync(token).ConfigureAwait(false)).ConfigureAwait(false))
                        return;
                    blnLoaded = true;
                    //Timekeeper.Finish("load_file");
                    await Program.OpenCharacter(objCharacter, token: token).ConfigureAwait(false);
                }
                catch
                {
                    if (!blnLoaded)
                        blnLoaded = await Program.OpenCharacters.RemoveAsync(objCharacter, token: CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    if (!blnLoaded)
                        await Program.OpenCharacters.RemoveAsync(objCharacter, token: token).ConfigureAwait(false);
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                    await cmdImport.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    await cmdSelectFile.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            await this.DoThreadSafeAsync(x => x.Close(), token).ConfigureAwait(false);
        }
    }
}
