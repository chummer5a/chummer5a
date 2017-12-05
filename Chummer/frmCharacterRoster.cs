using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    public partial class frmCharacterRoster : Form
    {
        List<CharacterCache> _lstCharacterCache = new List<CharacterCache>();
        private object _lstCharacterCacheLock = new object();
        HtmlToolTip tipTooltip = new HtmlToolTip();

        public frmCharacterRoster()
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);

            GlobalOptions.MRUChanged += PopulateCharacterList;
            treCharacterList.ItemDrag += treCharacterList_ItemDrag;
            treCharacterList.DragEnter += treCharacterList_DragEnter;
            treCharacterList.DragDrop += treCharacterList_DragDrop;
            treCharacterList.DragOver += treCharacterList_DragOver;
            LoadCharacters();
            MoveControls();
        }

        public void PopulateCharacterList()
        {
            //TODO: Cheaper way to do this than rebuilding the list every time?
            treCharacterList.Nodes.Clear();
            LoadCharacters();
            MoveControls();
        }

        private void LoadCharacters()
        {
            List<string> lstFavorites = GlobalOptions.ReadMRUList("stickymru");
            TreeNode objFavouriteNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_FavouriteCharacters")) { Tag = "Favourite" };

            List<string> lstRecents = GlobalOptions.ReadMRUList();

            List<string> lstWatch = new List<string>();
            TreeNode objWatchNode = null;
            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                string[] objFiles = Directory.GetFiles(GlobalOptions.CharacterRosterPath, "*.chum5");
                for (int i = 0; i < objFiles.Length; ++i)
                {
                    string strFile = objFiles[i];
                    // Make sure we're not loading a character that was already loaded by the MRU list.
                    if (lstFavorites.Contains(strFile) || lstRecents.Contains(strFile))
                        continue;
                    int intCachedCharacterIndex = _lstCharacterCache.FindIndex(x => x.FilePath == strFile);
                    if (intCachedCharacterIndex != -1)
                    {
                        foreach (TreeNode rootNode in treCharacterList.Nodes)
                        {
                            foreach (TreeNode objChildNode in rootNode.Nodes)
                            {
                                if (Convert.ToInt32(objChildNode.Tag) == intCachedCharacterIndex)
                                    goto CharacterAlreadyLoaded;
                            }
                        }
                    }
                    lstWatch.Add(strFile);
                    CharacterAlreadyLoaded:;
                }
            }
            if (lstWatch.Count > 0)
            {
                objWatchNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_WatchFolder")) { Tag = "Watch" };
            }

            // Add any characters that are open to the displayed list so we can have more than 10 characters listed
            foreach (Character objCharacter in GlobalOptions.MainForm.OpenCharacters)
            {
                string strFile = objCharacter.FileName;
                // Make sure we're not loading a character that was already loaded by the MRU list.
                if (lstFavorites.Contains(strFile) || lstRecents.Contains(strFile) || lstWatch.Contains(strFile))
                    continue;
                int intCachedCharacterIndex = _lstCharacterCache.FindIndex(x => x.FilePath == strFile);
                if (intCachedCharacterIndex != -1)
                {
                    foreach (TreeNode rootNode in treCharacterList.Nodes)
                    {
                        foreach (TreeNode objChildNode in rootNode.Nodes)
                        {
                            if (Convert.ToInt32(objChildNode.Tag) == intCachedCharacterIndex)
                                goto CharacterAlreadyLoaded;
                        }
                    }
                }
                lstRecents.Add(strFile);
                CharacterAlreadyLoaded:;
            }

            TreeNode objRecentNode = null;
            if (lstRecents.Count > 0)
            {
                objRecentNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_RecentCharacters")) { Tag = "Recent" };
            }

            TreeNode[] lstFavoritesNodes = new TreeNode[lstFavorites.Count];
            object lstFavoritesNodesLock = new object();
            TreeNode[] lstRecentsNodes = new TreeNode[lstRecents.Count];
            object lstRecentsNodesLock = new object();
            TreeNode[] lstWatchNodes = new TreeNode[lstWatch.Count];
            object lstWatchNodesLock = new object();
            Parallel.Invoke(
                () => {
                    if (objFavouriteNode != null)
                    {
                        Parallel.For(0, lstFavorites.Count, i =>
                        {
                            string strFile = lstFavorites[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstFavoritesNodesLock)
                                lstFavoritesNodes[i] = objNode;
                        });

                        for (int i = 0; i < lstFavoritesNodes.Length; i++)
                        {
                            TreeNode objNode = lstFavoritesNodes[i];
                            if (objNode != null)
                                objFavouriteNode.Nodes.Add(objNode);
                        }
                    }
                },
                () => {
                    if (objRecentNode != null)
                    {
                        Parallel.For(0, lstRecents.Count, i =>
                        {
                            string strFile = lstRecents[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstRecentsNodesLock)
                                lstRecentsNodes[i] = objNode;
                        });

                        for (int i = 0; i < lstRecentsNodes.Length; i++)
                        {
                            TreeNode objNode = lstRecentsNodes[i];
                            if (objNode != null)
                                objRecentNode.Nodes.Add(objNode);
                        }
                    }
                },
                () =>
                {
                    if (objWatchNode != null)
                    {
                        Parallel.For(0, lstWatch.Count, i =>
                        {
                            string strFile = lstWatch[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock (lstWatchNodesLock)
                                lstWatchNodes[i] = objNode;
                        });

                        for (int i = 0; i < lstWatchNodes.Length; i++)
                        {
                            TreeNode objNode = lstWatchNodes[i];
                            if (objNode != null)
                                objWatchNode.Nodes.Add(objNode);
                        }
                    }
                });
            treCharacterList.Nodes.Add(objFavouriteNode);
            if (objRecentNode != null)
                treCharacterList.Nodes.Add(objRecentNode);
            if (objWatchNode != null)
                treCharacterList.Nodes.Add(objWatchNode);
            treCharacterList.ExpandAll();
        }
        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="objParentNode"></param>
        private TreeNode CacheCharacter(string strFile)
        {
            if (!File.Exists(strFile))
                return null;
            XmlDocument objXmlSource = new XmlDocument();
            // If we run into any problems loading the character cache, fail out early.
            try
            {
                using (StreamReader sr = new StreamReader(strFile, true))
                {
                    objXmlSource.Load(sr);
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (XmlException)
            {
                return null;
            }
            CharacterCache objCache = new CharacterCache();
            XmlNode objXmlSourceNode = objXmlSource.SelectSingleNode("/character");
            if (objXmlSourceNode != null)
            {
                objCache.Description = objXmlSourceNode["description"]?.InnerText;
                objCache.BuildMethod = objXmlSourceNode["buildmethod"]?.InnerText;
                objCache.Background = objXmlSourceNode["background"]?.InnerText;
                objCache.CharacterNotes = objXmlSourceNode["notes"]?.InnerText;
                objCache.GameNotes = objXmlSourceNode["gamenotes"]?.InnerText;
                objCache.Concept = objXmlSourceNode["concept"]?.InnerText;
                objCache.Karma = objXmlSourceNode["totalkarma"]?.InnerText;
                objCache.Metatype = objXmlSourceNode["metatype"]?.InnerText;
                objCache.PlayerName = objXmlSourceNode["player"]?.InnerText;
                objCache.CharacterName = objXmlSourceNode["name"]?.InnerText;
                objCache.CharacterAlias = objXmlSourceNode["alias"]?.InnerText;
                objCache.Created = objXmlSourceNode["created"]?.InnerText == System.Boolean.TrueString;
                objCache.Essence = objXmlSourceNode["totaless"]?.InnerText;
                string strMugshotBase64 = objXmlSourceNode["mugshot"]?.InnerText;
                if (!string.IsNullOrEmpty(strMugshotBase64))
                {
                    objCache.Mugshot = strMugshotBase64.ToImage();
                }
                else
                {
                    int intMainMugshotIndex = -1;
                    objXmlSourceNode.TryGetInt32FieldQuickly("mainmugshotindex", ref intMainMugshotIndex);
                    XmlNodeList objXmlMugshotsList = objXmlSourceNode.SelectNodes("mugshots/mugshot");

                    if (intMainMugshotIndex >= 0 && intMainMugshotIndex < objXmlMugshotsList.Count)
                        objCache.Mugshot = objXmlMugshotsList[intMainMugshotIndex].InnerText?.ToImage();
                    else
                        objCache.Mugshot = null;
                }
            }
            objCache.FilePath = strFile;
            objCache.FileName = strFile.Substring(strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            TreeNode objNode = new TreeNode();

            objNode.Text = CalculatedName(objCache);
            objNode.ToolTipText = objCache.FilePath;
            lock (_lstCharacterCacheLock)
            {
                _lstCharacterCache.Add(objCache);
                objNode.Tag = _lstCharacterCache.IndexOf(objCache);
            }
            return objNode;
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object. 
        /// </summary>
        /// <param name="objCache"></param>
        /// <returns></returns>
        private static string CalculatedName(CharacterCache objCache)
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
                strBuildMethod = "Unknown build method";
            string strCreated = LanguageManager.GetString(objCache.Created ? "Title_CareerMode" : "Title_CreateMode");
            string strReturn = $"{strName} ({strBuildMethod} - {strCreated})";
            if (GlobalOptions.MainForm.OpenCharacters.Any(x => x.FileName == objCache.FilePath))
                strReturn = "* " + strReturn;
            return strReturn;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        private void UpdateCharacter(CharacterCache objCache)
        {
            if (objCache != null)
            {
                txtCharacterBio.Text = objCache.Description;
                txtCharacterBackground.Text = objCache.Background;
                txtCharacterNotes.Text = objCache.CharacterNotes;
                txtGameNotes.Text = objCache.GameNotes;
                txtCharacterConcept.Text = objCache.Concept;
                lblCareerKarma.Text = objCache.Karma;
                lblMetatype.Text = objCache.Metatype;
                lblPlayerName.Text = objCache.PlayerName;
                lblCharacterName.Text = objCache.CharacterName;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                lblEssence.Text = objCache.Essence;
                lblFilePath.Text = objCache.FileName;
                tipTooltip.SetToolTip(lblFilePath, objCache.FilePath);
                picMugshot.Image = objCache.Mugshot;
            }
            else
            {
                txtCharacterBio.Text = string.Empty;
                txtCharacterBackground.Text = string.Empty;
                txtCharacterNotes.Text = string.Empty;
                txtGameNotes.Text = string.Empty;
                txtCharacterConcept.Text = string.Empty;
                lblCareerKarma.Text = string.Empty;
                lblMetatype.Text = string.Empty;
                lblPlayerName.Text = string.Empty;
                lblCharacterName.Text = string.Empty;
                lblCharacterAlias.Text = string.Empty;
                lblEssence.Text = string.Empty;
                lblFilePath.Text = string.Empty;
                tipTooltip.SetToolTip(lblFilePath, string.Empty);
                picMugshot.Image = null;
            }
            picMugshot_SizeChanged(null, EventArgs.Empty);
        }

        #region Form Methods

        private void MoveControls()
        {
            int intWidth = 0;
            int intMargin = 0;
            foreach (TreeNode objNode in treCharacterList.Nodes)
            {
                intMargin = Math.Max(intMargin, objNode.Bounds.Left);
                intWidth =
                    (from TreeNode objChildNode in objNode.Nodes select objChildNode.Bounds.Right).Concat(new[] { intWidth })
                    .Max();
            }
            intWidth += intMargin;

            int intDifference = intWidth - treCharacterList.Width;
            treCharacterList.Width = intWidth;
            tabCharacterText.Left = treCharacterList.Width + 12;
            tabCharacterText.Width -= intDifference;

            lblPlayerNameLabel.Left = tabCharacterText.Left;
            lblCharacterNameLabel.Left = tabCharacterText.Left;
            lblCareerKarmaLabel.Left = tabCharacterText.Left;
            lblMetatypeLabel.Left = tabCharacterText.Left;
            lblCharacterAliasLabel.Left = tabCharacterText.Left;
            lblEssenceLabel.Left = tabCharacterText.Left;
            lblFilePathLabel.Left = tabCharacterText.Left;
            intWidth = lblPlayerNameLabel.Right;
            if (lblCareerKarmaLabel.Right > intWidth)
            {
                intWidth = lblCareerKarmaLabel.Right;
            }
            if (lblCareerKarmaLabel.Right > intWidth)
            {
                intWidth = lblCareerKarmaLabel.Right;
            }
            if (lblMetatypeLabel.Right > intWidth)
            {
                intWidth = lblMetatypeLabel.Right;
            }
            if (lblCharacterAliasLabel.Right > intWidth)
            {
                intWidth = lblCharacterAliasLabel.Right;
            }
            if (lblEssenceLabel.Right > intWidth)
            {
                intWidth = lblEssenceLabel.Right;
            }
            if (lblFilePathLabel.Right > intWidth)
            {
                intWidth = lblFilePathLabel.Right;
            }
            intWidth += 12;
            lblEssence.Left = intWidth;
            lblPlayerName.Left = intWidth;
            lblCareerKarma.Left = intWidth;
            lblCharacterAlias.Left = intWidth;
            lblMetatype.Left = intWidth;
            lblCharacterName.Left = intWidth;
            lblFilePath.Left = intWidth;
        }

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CharacterCache objCache = null;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                    objCache = _lstCharacterCache[intIndex];
            }
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                int intIndex = Convert.ToInt32(objSelectedNode.Tag);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                {
                    string strFile = _lstCharacterCache[intIndex]?.FilePath;
                    if (!string.IsNullOrEmpty(strFile))
                    {
                        Character objOpenCharacters = GlobalOptions.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                        if (objOpenCharacters == null || !GlobalOptions.MainForm.SwitchToOpenCharacter(objOpenCharacters))
                        {
                            Cursor = Cursors.WaitCursor;
                            objSelectedNode.Text = "* " + objSelectedNode.Text;
                            Character objOpenCharacter = frmMain.LoadCharacter(strFile);
                            Cursor = Cursors.Default;
                            GlobalOptions.MainForm.OpenCharacter(objOpenCharacter);
                        }
                    }
                }
            }
        }
        private void treCharacterList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                TreeNode objSelectedNode = treCharacterList.SelectedNode;
                if (objSelectedNode != null && objSelectedNode.Level > 0)
                {
                    RemoveSelected(objSelectedNode);
                }
            }
        }

        private void treCharacterList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treCharacterList_DragOver(object sender, DragEventArgs e)
        {
            TreeView treSenderView = sender as TreeView;
            if (treSenderView == null)
                return;
            Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = treSenderView.GetNodeAt(pt);
            if (objNode != null)
            {
                if (objNode.Parent != null)
                    objNode = objNode.Parent;
                if (objNode.Tag.ToString() != "Watch")
                {
                    objNode.BackColor = SystemColors.ControlDark;
                }
            }

            // Clear the background colour for all other Nodes.
            treCharacterList.ClearNodeBackground(objNode);
        }
        private void treCharacterList_DragDrop(object sender, DragEventArgs e)
        {
            // Do not allow the root element to be moved.
            if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 || treCharacterList.SelectedNode.Parent.Tag.ToString() == "Watch")
                return;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                TreeView treSenderView = sender as TreeView;
                if (treSenderView == null)
                    return;
                Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                if (nodDestinationNode.Level > 0)
                    nodDestinationNode = nodDestinationNode.Parent;
                if (nodDestinationNode.Tag.ToString() != "Watch")
                {
                    TreeNode nodNewNode = e.Data.GetData("System.Windows.Forms.TreeNode") as TreeNode;
                    if (nodNewNode == null)
                        return;

                    if (nodNewNode.Level == 0 || nodNewNode.Parent == nodDestinationNode)
                        return;
                    int intCharacterIndex;
                    if (int.TryParse(nodNewNode.Tag.ToString(), out intCharacterIndex) && intCharacterIndex >= 0 && intCharacterIndex < _lstCharacterCache.Count)
                    {
                        CharacterCache objCache = _lstCharacterCache[intCharacterIndex];

                        if (objCache == null)
                            return;
                        switch (nodDestinationNode.Tag.ToString())
                        {
                            case "Recent":
                                GlobalOptions.RemoveFromMRUList(objCache.FilePath, "stickymru", false);
                                GlobalOptions.AddToMRUList(objCache.FilePath);
                                break;
                            case "Favourite":
                                GlobalOptions.RemoveFromMRUList(objCache.FilePath, "mru", false);
                                GlobalOptions.AddToMRUList(objCache.FilePath, "stickymru");
                                break;
                        }
                    }
                }
            }
        }

        private void treCharacterList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void RemoveSelected(TreeNode sender)
        {
            if (sender != null)
            {
                int intIndex = Convert.ToInt32(sender.Tag);
                if (intIndex >= 0 && intIndex < _lstCharacterCache.Count)
                {
                    string strFile = _lstCharacterCache[intIndex]?.FilePath;
                    if (!string.IsNullOrEmpty(strFile))
                    {
                        GlobalOptions.RemoveFromMRUList(strFile, "mru", false);
                        GlobalOptions.RemoveFromMRUList(strFile, "stickymru");
                    }
                }
                sender.Remove();
            }
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
        }
        #endregion
        #region Classes
        /// <summary>
        /// Caches a subset of a full character's properties for loading purposes. 
        /// </summary>
        private class CharacterCache
        {
            internal string FilePath { get; set; }
            internal string FileName { get; set; }
            internal string Description { get; set; }
            internal string Background { get; set; }
            internal string GameNotes { get; set; }
            internal string CharacterNotes { get; set; }
            internal string Concept { get; set; }
            internal string Karma { get; set; }
            internal string Metatype { get; set; }
            internal string PlayerName { get; set; }
            internal string CharacterName { get; set; }
            internal string CharacterAlias { get; set; }
            internal string BuildMethod { get; set; }
            internal string Essence { get; set; }
            internal Image Mugshot { get; set; }
            internal bool Created { get; set; }
        }
        #endregion
    }
}
