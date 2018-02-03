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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    public partial class frmCharacterRoster : Form
    {
        private readonly ConcurrentDictionary<string, CharacterCache> _lstCharacterCache = new ConcurrentDictionary<string, CharacterCache>();
        private readonly HtmlToolTip tipTooltip = new HtmlToolTip();

        public frmCharacterRoster()
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            GlobalOptions.MRUChanged += PopulateCharacterList;
            treCharacterList.ItemDrag += treCharacterList_ItemDrag;
            treCharacterList.DragEnter += treCharacterList_DragEnter;
            treCharacterList.DragDrop += treCharacterList_DragDrop;
            treCharacterList.DragOver += treCharacterList_DragOver;
            LoadCharacters();
            MoveControls();
            ContextMenuStrip[] lstCMSToTranslate = new ContextMenuStrip[]
            {
                cmsRoster
            };

            foreach (ContextMenuStrip objCMS in lstCMSToTranslate)
            {
                if (objCMS != null)
                {
                    foreach (ToolStripMenuItem objItem in objCMS.Items.OfType<ToolStripMenuItem>())
                    {
                        LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
                    }
                }
            }
        }

        public void PopulateCharacterList()
        {
            SuspendLayout();
            //TODO: Cheaper way to do this than rebuilding the list every time?
            treCharacterList.Nodes.Clear();
            _lstCharacterCache.Clear();
            LoadCharacters();
            MoveControls();
            GC.Collect();
            ResumeLayout();
        }

        private void LoadCharacters()
        {
            List<string> lstFavorites = GlobalOptions.ReadMRUList("stickymru");
            TreeNode objFavouriteNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_FavouriteCharacters", GlobalOptions.Language)) { Tag = "Favourite" };

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
                    if (lstFavorites.Contains(strFile) ||
                        lstRecents.Contains(strFile) ||
                        (_lstCharacterCache.ContainsKey(strFile) && treCharacterList.FindNode(strFile) != null))
                        continue;

                    lstWatch.Add(strFile);
                }
            }
            if (lstWatch.Count > 0)
            {
                objWatchNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_WatchFolder", GlobalOptions.Language)) { Tag = "Watch" };
            }

            // Add any characters that are open to the displayed list so we can have more than 10 characters listed
            foreach (CharacterShared objCharacterForm in Program.MainForm.OpenCharacterForms)
            {
                string strFile = objCharacterForm.CharacterObject.FileName;
                // Make sure we're not loading a character that was already loaded by the MRU list.
                if (lstFavorites.Contains(strFile) ||
                    lstRecents.Contains(strFile) ||
                    lstWatch.Contains(strFile) ||
                    (_lstCharacterCache.ContainsKey(strFile) && treCharacterList.FindNode(strFile) != null))
                    continue;

                lstRecents.Add(strFile);
            }

            TreeNode objRecentNode = null;
            if (lstRecents.Count > 0)
            {
                objRecentNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_RecentCharacters", GlobalOptions.Language)) { Tag = "Recent" };
            }
            
            TreeNode[] lstFavoritesNodes = new TreeNode[lstFavorites.Count];
            object lstFavoritesNodesLock = new object();
            TreeNode[] lstRecentsNodes = new TreeNode[lstRecents.Count];
            object lstRecentsNodesLock = new object();
            TreeNode[] lstWatchNodes = new TreeNode[lstWatch.Count];
            object lstWatchNodesLock = new object();
            Parallel.Invoke(
                () => {
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
        private TreeNode CacheCharacter(string strFile)
        {
            if (!File.Exists(strFile))
                return null;
            XDocument xmlSource;
            // If we run into any problems loading the character cache, fail out early.
            try
            {
                using (StreamReader objStreamReader = new StreamReader(strFile, true))
                {
                    xmlSource = XDocument.Load(objStreamReader);
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
            XElement xmlSourceNode = xmlSource.Element("character");
            if (xmlSourceNode != null)
            {
                objCache.Description = xmlSourceNode.Element("description")?.Value;
                objCache.BuildMethod = xmlSourceNode.Element("buildmethod")?.Value;
                objCache.Background = xmlSourceNode.Element("background")?.Value;
                objCache.CharacterNotes = xmlSourceNode.Element("notes")?.Value;
                objCache.GameNotes = xmlSourceNode.Element("gamenotes")?.Value;
                objCache.Concept = xmlSourceNode.Element("concept")?.Value;
                objCache.Karma = xmlSourceNode.Element("totalkarma")?.Value;
                objCache.Metatype = xmlSourceNode.Element("metatype")?.Value;
                objCache.Metavariant = xmlSourceNode.Element("metavariant")?.Value;
                objCache.PlayerName = xmlSourceNode.Element("playername")?.Value;
                objCache.CharacterName = xmlSourceNode.Element("name")?.Value;
                objCache.CharacterAlias = xmlSourceNode.Element("alias")?.Value;
                objCache.Created = xmlSourceNode.Element("created")?.Value == bool.TrueString;
                objCache.Essence = xmlSourceNode.Element("totaless")?.Value;
                string strSettings = xmlSourceNode.Element("settings")?.Value ?? string.Empty;
                objCache.SettingsFile = !File.Exists(Path.Combine(Application.StartupPath, "settings", strSettings)) ? LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language) : strSettings;
                string strMugshotBase64 = xmlSourceNode.Element("mugshot")?.Value;
                if (!string.IsNullOrEmpty(strMugshotBase64))
                {
                    objCache.Mugshot = strMugshotBase64.ToImage();
                }
                else
                {
                    XElement xmlMainMugshotIndex = xmlSourceNode.Element("mainmugshotindex");
                    if (xmlMainMugshotIndex != null && int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) && intMainMugshotIndex >= 0)
                    {
                        XElement[] xmlMugshots = xmlSourceNode.Element("mugshots")?.Elements("mugshot").ToArray();
                        if (xmlMugshots != null && intMainMugshotIndex < xmlMugshots.Length)
                            objCache.Mugshot = xmlMugshots[intMainMugshotIndex].Value.ToImage();
                    }
                }
            }
            objCache.FilePath = strFile;
            objCache.FileName = strFile.Substring(strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            TreeNode objNode = new TreeNode
            {
                ContextMenuStrip = cmsRoster,
                Text = CalculatedName(objCache),
                ToolTipText = objCache.FilePath.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>'),
                Tag = strFile
            };
            _lstCharacterCache.TryAdd(strFile, objCache);
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
                    strName = LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language);
            }
            string strBuildMethod = LanguageManager.GetString("String_" + objCache.BuildMethod, GlobalOptions.Language, false);
            if (string.IsNullOrEmpty(strBuildMethod))
                strBuildMethod = "Unknown build method";
            string strCreated = LanguageManager.GetString(objCache.Created ? "Title_CareerMode" : "Title_CreateMode", GlobalOptions.Language);
            string strReturn = $"{strName} ({strBuildMethod} - {strCreated})";
            if (Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject.FileName == objCache.FilePath))
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
                lblPlayerName.Text = objCache.PlayerName;
                lblCharacterName.Text = objCache.CharacterName;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                lblEssence.Text = objCache.Essence;
                lblFilePath.Text = objCache.FileName;
                lblSettings.Text = objCache.SettingsFile;
                tipTooltip.SetToolTip(lblFilePath, objCache.FilePath.CheapReplace(Application.StartupPath, () => '<' + Application.ProductName + '>'));
                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml");
                XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCache.Metatype + "\"]");
                if (objMetatypeNode == null)
                {
                    objMetatypeDoc = XmlManager.Load("critters.xml");
                    objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objCache.Metatype + "\"]");
                }

                string strMetatype = objMetatypeNode?["translate"]?.InnerText ?? objCache.Metatype;

                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                {
                    objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + objCache.Metavariant + "\"]");

                    strMetatype += " (" + (objMetatypeNode?["translate"]?.InnerText ?? objCache.Metavariant) + ')';
                }
                lblMetatype.Text = strMetatype;
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
                lblSettings.Text = string.Empty;
                picMugshot.Image = null;
            }
            ProcessMugshotSizeMode();
        }

        #region Form Methods

        private void MoveControls()
        {
            int intWidth = 0;
            int intMargin = 0;
            foreach (TreeNode objNode in treCharacterList.Nodes)
            {
                intMargin = Math.Max(intMargin, objNode.Bounds.Left);
                intWidth = Math.Max(0, objNode.GetRightMostEdge());
            }
            intWidth += intMargin;

            int intDifference = intWidth - treCharacterList.Width;
            treCharacterList.Width = intWidth;
            tabCharacterText.Left = treCharacterList.Width + 12;
            tabCharacterText.Width -= intDifference;
            tlpCharacterBlock.Left = tabCharacterText.Left;
        }

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CharacterCache objCache = null;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                _lstCharacterCache.TryGetValue(objSelectedNode.Tag.ToString(), out objCache);
            }
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                string strFile = objSelectedNode.Tag.ToString();
                if (!string.IsNullOrEmpty(strFile))
                {
                    Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    Cursor = Cursors.WaitCursor;
                    if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                    {
                        objOpenCharacter = Program.MainForm.LoadCharacter(strFile);
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                        if (_lstCharacterCache.TryGetValue(strFile, out CharacterCache objCache))
                            objSelectedNode.Text = CalculatedName(objCache);
                    }
                    Cursor = Cursors.Default;
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
            if (!(sender is TreeView treSenderView))
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
                if (!(sender is TreeView treSenderView))
                    return;
                Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                if (nodDestinationNode.Level > 0)
                    nodDestinationNode = nodDestinationNode.Parent;
                string strDestinationNode = nodDestinationNode.Tag.ToString();
                if (strDestinationNode != "Watch")
                {
                    if (!(e.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                        return;

                    if (nodNewNode.Level == 0 || nodNewNode.Parent == nodDestinationNode)
                        return;
                    if (_lstCharacterCache.TryGetValue(nodNewNode.Tag.ToString(), out CharacterCache objCache) && objCache != null)
                    {
                        switch (strDestinationNode)
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

        private void RemoveSelected(TreeNode objSender)
        {
            if (objSender != null)
            {
                string strFile = objSender.Tag.ToString();
                if (!string.IsNullOrEmpty(strFile))
                {
                    GlobalOptions.RemoveFromMRUList(strFile, "mru", false);
                    GlobalOptions.RemoveFromMRUList(strFile, "stickymru");
                }
                objSender.Remove();
            }
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            ProcessMugshotSizeMode();
        }

        private void ProcessMugshotSizeMode()
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
        private sealed class CharacterCache
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
            internal string Metavariant { get; set; }
            internal string PlayerName { get; set; }
            internal string CharacterName { get; set; }
            internal string CharacterAlias { get; set; }
            internal string BuildMethod { get; set; }
            internal string Essence { get; set; }
            internal Image Mugshot { get; set; }
            internal bool Created { get; set; }
            public string SettingsFile { get; set; }
        }
        #endregion

        private void tsDelete_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode != null && objSelectedNode.Level > 0)
            {
                RemoveSelected(objSelectedNode);
            }
        }

        private void tsSort_Click(object sender, EventArgs e)
        {
            //treCharacterList.SortCustom();
            //TODO: Need to sort them permanently, above sort method just sorts the treenodes.
        }

        private void tsToggleFav_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t == null) return;
            if (_lstCharacterCache.TryGetValue(t.Tag.ToString(), out CharacterCache objCache) && objCache != null)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.RemoveFromMRUList(objCache.FilePath, "mru", false);
                        GlobalOptions.AddToMRUList(objCache.FilePath, "stickymru");
                        break;
                    case "Favourite":
                        GlobalOptions.RemoveFromMRUList(objCache.FilePath, "stickymru", false);
                        GlobalOptions.AddToMRUList(objCache.FilePath);
                        break;
                    case "Watch":
                        GlobalOptions.AddToMRUList(objCache.FilePath, "stickymru");
                        break;
                }
                treCharacterList.SelectedNode = t;
            }
        }

        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            // Generic event for all TreeViews to allow right-clicking to select a TreeNode so the proper ContextMenu is shown.
            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //{
            TreeView objTree = (TreeView)sender;
            objTree.SelectedNode = objTree.HitTest(e.X, e.Y).Node;
            //}
            if (ModifierKeys == Keys.Control)
            {
                if (!objTree.SelectedNode.IsExpanded)
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.ExpandAll();
                    }
                }
                else
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.Collapse();
                    }
                }
            }
        }

        private void tsCloseOpenCharacter_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            string strFile = objSelectedNode.Tag.ToString();
            if (string.IsNullOrEmpty(strFile))
                return;
            Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
            Cursor = Cursors.WaitCursor;
            if (objOpenCharacter != null)
            {
                Program.MainForm.OpenCharacters.Remove(objOpenCharacter);
                Program.MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objOpenCharacter)?.Close();
                Program.MainForm.CharacterRoster.PopulateCharacterList();
                objOpenCharacter.DeleteCharacter();
            }
            Cursor = Cursors.Default;
        }
    }
}
