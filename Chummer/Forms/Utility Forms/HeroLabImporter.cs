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

namespace Chummer
{
    public partial class HeroLabImporter : Form
    {
        private readonly ThreadSafeList<HeroLabCharacterCache> _lstCharacterCache = new ThreadSafeList<HeroLabCharacterCache>(1);
        private readonly LockingDictionary<string, Bitmap> _dicImages = new LockingDictionary<string, Bitmap>();

        public HeroLabImporter()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdSelectFile_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to possess.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
                   {
                       Filter = await LanguageManager.GetStringAsync("DialogFilter_HeroLab") + '|'
                           + await LanguageManager.GetStringAsync("DialogFilter_All"),
                       Multiselect = false
                   })
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                using (CursorWait.New(this))
                {
                    string strSelectedFile = openFileDialog.FileName;
                    TreeNode objNode = await CacheCharacters(strSelectedFile);
                    if (objNode != null)
                    {
                        await treCharacterList.DoThreadSafeAsync(x =>
                        {
                            x.Nodes.Clear();
                            x.Nodes.Add(objNode);
                            x.SelectedNode = objNode.Nodes.Count > 0 ? objNode.Nodes[0] : objNode;
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        private async Task<TreeNode> CacheCharacters(string strFile)
        {
            if (!File.Exists(strFile))
            {
                Program.ShowMessageBox(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                                  await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"), strFile));
                return null;
            }

            ThreadSafeList<XPathNavigator> lstCharacterXmlStatblocks = new ThreadSafeList<XPathNavigator>(3);
            try
            {
                try
                {
                    using (ZipArchive zipArchive
                           = ZipFile.Open(strFile, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                    {
                        // NOTE: Cannot parallelize because ZipFile.Open creates one handle on the entire zip file that gets messed up if we try to get it to read multiple files at once
                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        {
                            string strEntryFullName = entry.FullName;
                            if (strEntryFullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                                && strEntryFullName.StartsWith("statblocks_xml", StringComparison.Ordinal))
                            {
                                // If we run into any problems loading the character cache, fail out early.
                                try
                                {
                                    using (StreamReader sr = new StreamReader(entry.Open(), true))
                                    {
                                        await Task.Run(() =>
                                        {
                                            XPathDocument xmlSourceDoc;
                                            using (XmlReader objXmlReader
                                                   = XmlReader.Create(sr, GlobalSettings.SafeXmlReaderSettings))
                                                xmlSourceDoc = new XPathDocument(objXmlReader);
                                            XPathNavigator objToAdd = xmlSourceDoc.CreateNavigator();
                                            lstCharacterXmlStatblocks.Add(objToAdd);
                                        });
                                    }
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
                                string strKey = Path.GetFileName(strEntryFullName);
                                using (Bitmap bmpMugshot = new Bitmap(entry.Open(), true))
                                {
                                    Bitmap bmpNewMugshot = bmpMugshot.PixelFormat == PixelFormat.Format32bppPArgb
                                        ? bmpMugshot.Clone() as Bitmap // Clone makes sure file handle is closed
                                        : bmpMugshot.ConvertPixelFormat(PixelFormat.Format32bppPArgb);
                                    while (!await _dicImages.TryAddAsync(strKey, bmpNewMugshot))
                                    {
                                        (bool blnSuccess, Bitmap bmpOldMugshot) =
                                            await _dicImages.TryRemoveAsync(strKey);
                                        if (blnSuccess)
                                            bmpOldMugshot?.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    Program.ShowMessageBox(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                                      await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"),
                                      strFile));
                    return null;
                }
                catch (NotSupportedException)
                {
                    Program.ShowMessageBox(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                                      await LanguageManager.GetStringAsync("Message_File_Cannot_Be_Accessed"),
                                      strFile));
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    Program.ShowMessageBox(
                        this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                    return null;
                }

                string strFileText
                    = await strFile.CheapReplaceAsync(Utils.GetStartupPath, () => '<' + Application.ProductName + '>');
                TreeNode nodRootNode = new TreeNode
                {
                    Text = strFileText,
                    ToolTipText = strFileText
                };

                XPathNavigator xmlMetatypesDocument = await XmlManager.LoadXPathAsync("metatypes.xml");
                foreach (XPathNavigator xmlCharacterDocument in lstCharacterXmlStatblocks)
                {
                    XPathNavigator xmlBaseCharacterNode
                        = xmlCharacterDocument.SelectSingleNode("/document/public/character");
                    if (xmlBaseCharacterNode != null)
                    {
                        HeroLabCharacterCache objCache = new HeroLabCharacterCache
                        {
                            PlayerName = xmlBaseCharacterNode.SelectSingleNode("@playername")?.Value ?? string.Empty
                        };
                        string strNameString = xmlBaseCharacterNode.SelectSingleNode("@name")?.Value ?? string.Empty;
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

                        string strRaceString = xmlBaseCharacterNode.SelectSingleNode("race/@name")?.Value;
                        if (strRaceString == "Metasapient")
                            strRaceString = "A.I.";
                        if (!string.IsNullOrEmpty(strRaceString))
                        {
                            foreach (XPathNavigator xmlMetatype in xmlMetatypesDocument.Select(
                                         "/chummer/metatypes/metatype"))
                            {
                                string strMetatypeName = xmlMetatype.SelectSingleNode("name")?.Value ?? string.Empty;
                                if (strMetatypeName == strRaceString)
                                {
                                    objCache.Metatype = strMetatypeName;
                                    objCache.Metavariant = "None";
                                    break;
                                }

                                foreach (XPathNavigator xmlMetavariant in
                                         await xmlMetatype.SelectAndCacheExpressionAsync("metavariants/metavariant"))
                                {
                                    string strMetavariantName
                                        = xmlMetavariant.SelectSingleNode("name")?.Value ?? string.Empty;
                                    if (strMetavariantName == strRaceString)
                                    {
                                        objCache.Metatype = strMetatypeName;
                                        objCache.Metavariant = strMetavariantName;
                                        break;
                                    }
                                }
                            }
                        }

                        objCache.Description = xmlBaseCharacterNode.SelectSingleNode("personal/description")?.Value;
                        objCache.Karma = xmlBaseCharacterNode.SelectSingleNode("karma/@total")?.Value ?? "0";
                        objCache.Essence = xmlBaseCharacterNode
                                           .SelectSingleNode("attributes/attribute[@name = \"Essence\"]/@text")?.Value;
                        objCache.BuildMethod
                            = xmlBaseCharacterNode.SelectSingleNode("creation/bp/@total")?.ValueAsInt <= 100
                                ? CharacterBuildMethod.Priority
                                : CharacterBuildMethod.Karma;

                        string strSettingsSummary =
                            xmlBaseCharacterNode.SelectSingleNode("settings/@summary")?.Value;
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
                                = await xmlBaseCharacterNode.SelectAndCacheExpressionAsync("journals/journal");
                            if (xmlJournalEntries?.Count > 1)
                            {
                                objCache.Created = true;
                            }
                            else if (xmlJournalEntries?.Count == 1
                                     && xmlJournalEntries.Current?.SelectSingleNode("@name")?.Value != "Title")
                            {
                                objCache.Created = true;
                            }
                        }

                        string strImageString = xmlBaseCharacterNode.SelectSingleNode("images/image/@filename")?.Value;
                        if (!string.IsNullOrEmpty(strImageString))
                        {
                            (bool blnSuccess, Bitmap objTemp) = await _dicImages.TryGetValueAsync(strImageString);
                            if (blnSuccess)
                            {
                                objCache.Mugshot = objTemp;
                            }
                        }

                        objCache.FilePath = strFile;
                        TreeNode objNode = new TreeNode
                        {
                            Text = await CalculatedName(objCache),
                            ToolTipText = await strFile.CheapReplaceAsync(Utils.GetStartupPath,
                                                                          () => '<' + Application.ProductName + '>')
                        };
                        nodRootNode.Nodes.Add(objNode);

                        await _lstCharacterCache.AddAsync(objCache);
                        objNode.Tag = await _lstCharacterCache.IndexOfAsync(objCache);
                    }
                }

                nodRootNode.Expand();
                return nodRootNode;
            }
            finally
            {
                await lstCharacterXmlStatblocks.DisposeAsync();
            }
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
        /// <returns></returns>
        private static async Task<string> CalculatedName(HeroLabCharacterCache objCache)
        {
            string strName = objCache.CharacterAlias;
            if (string.IsNullOrEmpty(strName))
            {
                strName = objCache.CharacterName;
                if (string.IsNullOrEmpty(strName))
                    strName = await LanguageManager.GetStringAsync("String_UnnamedCharacter");
            }
            string strBuildMethod = await LanguageManager.GetStringAsync("String_" + objCache.BuildMethod, false);
            if (string.IsNullOrEmpty(strBuildMethod))
                strBuildMethod = await LanguageManager.GetStringAsync("String_Unknown");
            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            strName += strSpace + '(' + strBuildMethod + strSpace + '-' + strSpace
                       + await LanguageManager.GetStringAsync(objCache.Created ? "Title_CareerMode" : "Title_CreateMode") + ')';
            return strName;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        private async Task UpdateCharacter(HeroLabCharacterCache objCache)
        {
            if (objCache != null)
            {
                await txtCharacterBio.DoThreadSafeAsync(x => x.Text = objCache.Description);

                string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");

                await lblCharacterName.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.CharacterName;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                });
                await lblCharacterNameLabel.DoThreadSafeAsync(x => x.Visible = true);

                await lblCharacterAlias.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.CharacterAlias;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                });
                await lblCharacterAliasLabel.DoThreadSafeAsync(x => x.Visible = true);

                await lblPlayerName.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.PlayerName;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                });
                await lblPlayerNameLabel.DoThreadSafeAsync(x => x.Visible = true);

                string strNone = await LanguageManager.GetStringAsync("String_None");
                await lblCareerKarma.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.Karma;
                    if (string.IsNullOrEmpty(x.Text) || x.Text == 0.ToString(GlobalSettings.CultureInfo))
                        x.Text = strNone;
                    x.Visible = true;
                });
                await lblCareerKarmaLabel.DoThreadSafeAsync(x => x.Visible = true);

                await lblEssence.DoThreadSafeAsync(x =>
                {
                    x.Text = objCache.Essence;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                });
                await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = true);

                await picMugshot.DoThreadSafeAsync(x => x.Image = objCache.Mugshot);

                // Populate character information fields.
                XPathNavigator objMetatypeDoc = await XmlManager.LoadXPathAsync("metatypes.xml");
                XPathNavigator objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + ']');
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = await XmlManager.LoadXPathAsync("critters.xml");
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + ']');
                }

                string strMetatype = objMetatypeNode != null
                    ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                      ?? objCache.Metatype
                    : objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + ']');

                    strMetatype += " (" + (objMetatypeNode != null
                        ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                          ?? objCache.Metavariant
                        : objCache.Metavariant) + ')';
                }

                await lblMetatype.DoThreadSafeAsync(x =>
                {
                    x.Text = strMetatype;
                    if (string.IsNullOrEmpty(x.Text))
                        x.Text = strUnknown;
                    x.Visible = true;
                });
                await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = true);

                await cmdImport.DoThreadSafeAsync(x => x.Enabled = true);
            }
            else
            {
                await txtCharacterBio.DoThreadSafeAsync(x => x.Text = string.Empty);

                await lblCharacterNameLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblCharacterName.DoThreadSafeAsync(x => x.Visible = false);
                await lblCharacterAliasLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblCharacterAlias.DoThreadSafeAsync(x => x.Visible = false);
                await lblPlayerNameLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblPlayerName.DoThreadSafeAsync(x => x.Visible = false);
                await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblMetatype.DoThreadSafeAsync(x => x.Visible = false);
                await lblCareerKarmaLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblCareerKarma.DoThreadSafeAsync(x => x.Visible = false);
                await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = false);
                await lblEssence.DoThreadSafeAsync(x => x.Visible = false);

                await picMugshot.DoThreadSafeAsync(x => x.Image = null);
                await cmdImport.DoThreadSafeAsync(x => x.Enabled = false);
            }
            picMugshot_SizeChanged(null, EventArgs.Empty);
        }

        #region Form Methods

        private async void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            HeroLabCharacterCache objCache = null;
            TreeNode objSelectedNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode);
            if (objSelectedNode?.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag, GlobalSettings.InvariantCultureInfo);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                    objCache = _lstCharacterCache[intIndex];
            }
            await UpdateCharacter(objCache);
            await treCharacterList.DoThreadSafeAsync(x => x.ClearNodeBackground(x.SelectedNode));
        }

        private async void cmdImport_Click(object sender, EventArgs e)
        {
            await DoImport();
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed() || picMugshot.IsNullOrDisposed())
                return;
            picMugshot.DoThreadSafe(x =>
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
            });
        }

        #endregion Form Methods

        private async Task DoImport(CancellationToken token = default)
        {
            TreeNode objSelectedNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, token);
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
            using (CursorWait.New(this))
            {
                bool blnLoaded = false;
                Character objCharacter = new Character();
                try
                {
                    Program.OpenCharacters.Add(objCharacter);

                    CharacterSettings objHeroLabSettings =
                        SettingsManager.LoadedCharacterSettings.Values.FirstOrDefault(
                            x => x.Name == objCache.SettingsName && x.BuildMethod == objCache.BuildMethod);
                    if (objHeroLabSettings != null)
                    {
                        objCharacter.SettingsKey = objHeroLabSettings.DictionaryKey;
                    }
                    else
                    {
                        objHeroLabSettings = SettingsManager.LoadedCharacterSettings.Values.FirstOrDefault(
                            x => x.Name.Contains(objCache.SettingsName) && x.BuildMethod == objCache.BuildMethod);
                        if (objHeroLabSettings != null)
                        {
                            objCharacter.SettingsKey = objHeroLabSettings.DictionaryKey;
                        }
                        else
                        {
                            (bool blnSuccess, CharacterSettings objDefaultCharacterSettings)
                                = await SettingsManager.LoadedCharacterSettings.TryGetValueAsync(
                                    GlobalSettings.DefaultCharacterSetting);
                            if (blnSuccess && objCache.BuildMethod.UsesPriorityTables()
                                == objDefaultCharacterSettings.BuildMethod.UsesPriorityTables())
                            {
                                objCharacter.SettingsKey = objDefaultCharacterSettings.DictionaryKey;
                            }
                            else
                            {
                                objCharacter.SettingsKey = SettingsManager.LoadedCharacterSettings.Values
                                                                          .FirstOrDefault(
                                                                              x => x.BuiltInOption
                                                                                  && x.BuildMethod == objCache.BuildMethod)
                                                                          ?.DictionaryKey
                                                           ?? SettingsManager.LoadedCharacterSettings.Values
                                                                             .FirstOrDefault(
                                                                                 x => x.BuiltInOption
                                                                                     && x.BuildMethod.UsesPriorityTables()
                                                                                     == objCache.BuildMethod
                                                                                         .UsesPriorityTables())
                                                                             ?.DictionaryKey
                                                           ?? GlobalSettings.DefaultCharacterSetting;
                            }
                        }
                    }
                    
                    using (SelectBuildMethod frmPickBP = await this.DoThreadSafeFuncAsync(() => new SelectBuildMethod(objCharacter, true), token))
                    {
                        await frmPickBP.ShowDialogSafeAsync(this);
                        if (frmPickBP.DialogResult != DialogResult.OK)
                            return;
                    }

                    //Timekeeper.Start("load_file");
                    if (!await objCharacter.LoadFromHeroLabFileAsync(strFile, strCharacterId, objCharacter.SettingsKey))
                        return;
                    blnLoaded = true;
                    //Timekeeper.Finish("load_file");
                    await Program.OpenCharacter(objCharacter);
                }
                finally
                {
                    await cmdImport.DoThreadSafeAsync(x => x.Enabled = true, token);
                    await cmdSelectFile.DoThreadSafeAsync(x => x.Enabled = true, token);
                    if (!blnLoaded)
                        Program.OpenCharacters.Remove(objCharacter);
                }
            }

            Close();
        }
    }
}
