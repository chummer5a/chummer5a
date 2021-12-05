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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmHeroLabImporter : Form
    {
        private readonly ThreadSafeList<HeroLabCharacterCache> _lstCharacterCache = new ThreadSafeList<HeroLabCharacterCache>(1);
        private readonly LockingDictionary<string, Bitmap> _dicImages = new LockingDictionary<string, Bitmap>();

        public frmHeroLabImporter()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void cmdSelectFile_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to possess.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_HeroLab") + '|' + LanguageManager.GetString("DialogFilter_All"),
                Multiselect = false
            })
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                using (new CursorWait(this))
                {
                    string strSelectedFile = openFileDialog.FileName;
                    TreeNode objNode = CacheCharacters(strSelectedFile);
                    if (objNode != null)
                    {
                        treCharacterList.Nodes.Clear();
                        treCharacterList.Nodes.Add(objNode);
                        treCharacterList.SelectedNode = objNode.Nodes.Count > 0 ? objNode.Nodes[0] : objNode;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        private TreeNode CacheCharacters(string strFile)
        {
            if (!File.Exists(strFile))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_File_Cannot_Be_Accessed") + Environment.NewLine + Environment.NewLine + strFile);
                return null;
            }

            using (ThreadSafeList<XPathNavigator> lstCharacterXmlStatblocks = new ThreadSafeList<XPathNavigator>(3))
            {
                try
                {
                    using (ZipArchive zipArchive
                        = ZipFile.Open(strFile, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                    {
                        Parallel.ForEach(zipArchive.Entries, entry =>
                        {
                            string strEntryFullName = entry.FullName;
                            if (strEntryFullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                                && strEntryFullName.StartsWith("statblocks_xml", StringComparison.Ordinal))
                            {
                                // If we run into any problems loading the character cache, fail out early.
                                try
                                {
                                    XPathDocument xmlSourceDoc;
                                    using (StreamReader sr = new StreamReader(entry.Open(), true))
                                    using (XmlReader objXmlReader
                                        = XmlReader.Create(sr, GlobalSettings.SafeXmlReaderSettings))
                                        xmlSourceDoc = new XPathDocument(objXmlReader);
                                    XPathNavigator objToAdd = xmlSourceDoc.CreateNavigator();
                                    lstCharacterXmlStatblocks.Add(objToAdd);
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
                                    while (!_dicImages.TryAdd(strKey, bmpNewMugshot))
                                    {
                                        if (_dicImages.TryRemove(strKey, out Bitmap bmpOldMugshot))
                                            bmpOldMugshot?.Dispose();
                                    }
                                }
                            }
                        });
                    }
                }
                catch (IOException)
                {
                    Program.MainForm.ShowMessageBox(
                        this,
                        LanguageManager.GetString("Message_File_Cannot_Be_Accessed") + Environment.NewLine
                        + Environment.NewLine + strFile);
                    return null;
                }
                catch (NotSupportedException)
                {
                    Program.MainForm.ShowMessageBox(
                        this,
                        LanguageManager.GetString("Message_File_Cannot_Be_Accessed") + Environment.NewLine
                        + Environment.NewLine + strFile);
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    Program.MainForm.ShowMessageBox(
                        this, LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                    return null;
                }

                string strFileText
                    = strFile.CheapReplace(Application.StartupPath, () => "<" + Application.ProductName + ">");
                TreeNode nodRootNode = new TreeNode
                {
                    Text = strFileText,
                    ToolTipText = strFileText
                };

                XPathNavigator xmlMetatypesDocument = XmlManager.LoadXPath("metatypes.xml");
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
                                    xmlMetatype.SelectAndCacheExpression("metavariants/metavariant"))
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
                            = xmlBaseCharacterNode.SelectSingleNode("creation/bp/@total")?.Value == "25"
                                ? nameof(CharacterBuildMethod.Priority)
                                : nameof(CharacterBuildMethod.Karma);

                        objCache.Created = objCache.Karma != "0";
                        if (!objCache.Created)
                        {
                            XPathNodeIterator xmlJournalEntries = xmlBaseCharacterNode.SelectAndCacheExpression("journals/journal");
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
                        if (!string.IsNullOrEmpty(strImageString)
                            && _dicImages.TryGetValue(strImageString, out Bitmap objTemp))
                        {
                            objCache.Mugshot = objTemp;
                        }

                        objCache.FilePath = strFile;
                        TreeNode objNode = new TreeNode
                        {
                            Text = CalculatedName(objCache),
                            ToolTipText = strFile.CheapReplace(Application.StartupPath,
                                                               () => "<" + Application.ProductName + ">")
                        };
                        nodRootNode.Nodes.Add(objNode);

                        _lstCharacterCache.Add(objCache);
                        objNode.Tag = _lstCharacterCache.IndexOf(objCache);
                    }
                }

                nodRootNode.Expand();
                return nodRootNode;
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
            internal string BuildMethod { get; set; }
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
        private static string CalculatedName(HeroLabCharacterCache objCache)
        {
            string strName = objCache.CharacterAlias;
            if (string.IsNullOrEmpty(strName))
            {
                strName = objCache.CharacterName;
                if (string.IsNullOrEmpty(strName))
                    strName = LanguageManager.GetString("String_UnnamedCharacter");
            }
            string strBuildMethod = LanguageManager.GetString("String_" + objCache.BuildMethod, false);
            if (string.IsNullOrEmpty(strBuildMethod))
                strBuildMethod = LanguageManager.GetString("String_Unknown");
            strName += string.Format("{0}({1}{0}-{0}{2})",
                LanguageManager.GetString("String_Space"),
                strBuildMethod,
                LanguageManager.GetString(objCache.Created ? "Title_CareerMode" : "Title_CreateMode"));
            return strName;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        private void UpdateCharacter(HeroLabCharacterCache objCache)
        {
            if (objCache != null)
            {
                txtCharacterBio.Text = objCache.Description;

                string strUnknown = LanguageManager.GetString("String_Unknown");
                string strNone = LanguageManager.GetString("String_None");

                lblCharacterName.Text = objCache.CharacterName;
                if (string.IsNullOrEmpty(lblCharacterName.Text))
                    lblCharacterName.Text = strUnknown;
                lblCharacterNameLabel.Visible = !string.IsNullOrEmpty(lblCharacterName.Text);
                lblCharacterName.Visible = !string.IsNullOrEmpty(lblCharacterName.Text);

                lblCharacterAlias.Text = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(lblCharacterAlias.Text))
                    lblCharacterAlias.Text = strUnknown;
                lblCharacterAliasLabel.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text);
                lblCharacterAlias.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text);

                lblPlayerName.Text = objCache.PlayerName;
                if (string.IsNullOrEmpty(lblPlayerName.Text))
                    lblPlayerName.Text = strUnknown;
                lblPlayerNameLabel.Visible = !string.IsNullOrEmpty(lblPlayerName.Text);
                lblPlayerName.Visible = !string.IsNullOrEmpty(lblPlayerName.Text);

                lblCareerKarma.Text = objCache.Karma;
                if (string.IsNullOrEmpty(lblCareerKarma.Text) || lblCareerKarma.Text == 0.ToString(GlobalSettings.CultureInfo))
                    lblCareerKarma.Text = strNone;
                lblCareerKarmaLabel.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text);
                lblCareerKarma.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text);

                lblEssence.Text = objCache.Essence;
                if (string.IsNullOrEmpty(lblEssence.Text))
                    lblEssence.Text = strUnknown;
                lblEssenceLabel.Visible = !string.IsNullOrEmpty(lblEssence.Text);
                lblEssence.Visible = !string.IsNullOrEmpty(lblEssence.Text);

                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XPathNavigator objMetatypeDoc = XmlManager.LoadXPath("metatypes.xml");
                XPathNavigator objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + "]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.LoadXPath("critters.xml");
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype.CleanXPath() + "]");
                }

                string strMetatype = objMetatypeNode?.SelectSingleNode("translate")?.Value ?? objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + "]");

                    strMetatype += " (" + (objMetatypeNode?.SelectSingleNode("translate")?.Value ?? objCache.Metavariant) + ')';
                }
                lblMetatype.Text = strMetatype;
                if (string.IsNullOrEmpty(lblMetatype.Text))
                    lblMetatype.Text = strUnknown;

                lblMetatypeLabel.Visible = !string.IsNullOrEmpty(lblMetatype.Text);
                lblMetatype.Visible = !string.IsNullOrEmpty(lblMetatype.Text);

                cmdImport.Enabled = true;
            }
            else
            {
                txtCharacterBio.Text = string.Empty;

                lblCharacterNameLabel.Visible = false;
                lblCharacterName.Visible = false;
                lblCharacterAliasLabel.Visible = false;
                lblCharacterAlias.Visible = false;
                lblPlayerNameLabel.Visible = false;
                lblPlayerName.Visible = false;
                lblMetatypeLabel.Visible = false;
                lblMetatype.Visible = false;
                lblCareerKarmaLabel.Visible = false;
                lblCareerKarma.Visible = false;
                lblEssenceLabel.Visible = false;
                lblEssence.Visible = false;

                picMugshot.Image = null;
                cmdImport.Enabled = false;
            }
            picMugshot_SizeChanged(null, EventArgs.Empty);
        }

        #region Form Methods

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            HeroLabCharacterCache objCache = null;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode?.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag, GlobalSettings.InvariantCultureInfo);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                    objCache = _lstCharacterCache[intIndex];
            }
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            DoImport();
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            DoImport();
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed() || picMugshot.IsNullOrDisposed())
                return;
            try
            {
                picMugshot.SizeMode = picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width
                    ? PictureBoxSizeMode.CenterImage
                    : PictureBoxSizeMode.Zoom;
            }
            catch (ArgumentException) // No other way to catch when the Image is not null, but is disposed
            {
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        #endregion Form Methods

        private void DoImport()
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            int intIndex = Convert.ToInt32(objSelectedNode.Tag, GlobalSettings.InvariantCultureInfo);
            if (intIndex < 0 || intIndex >= _lstCharacterCache.Count)
                return;
            string strFile = _lstCharacterCache[intIndex]?.FilePath;
            string strCharacterId = _lstCharacterCache[intIndex]?.CharacterId;
            if (string.IsNullOrEmpty(strFile) || string.IsNullOrEmpty(strCharacterId))
                return;
            using (new CursorWait(this))
            {
                cmdImport.Enabled = false;
                cmdSelectFile.Enabled = false;
                Character objCharacter = new Character();
                Program.MainForm.OpenCharacters.Add(objCharacter);
                //Timekeeper.Start("load_file");
                bool blnLoaded = objCharacter.LoadFromHeroLabFile(strFile, strCharacterId);
                //Timekeeper.Finish("load_file");
                if (!blnLoaded)
                {
                    Program.MainForm.OpenCharacters.Remove(objCharacter);
                    cmdImport.Enabled = true;
                    cmdSelectFile.Enabled = true;
                    return;
                }

                Program.MainForm.OpenCharacter(objCharacter);
            }

            Close();
        }
    }
}
