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
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Chummer.Plugins;
using NLog;

namespace Chummer
{
    public partial class frmCharacterRoster : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //private readonly ConcurrentDictionary<string, CharacterCache> _lstCharacterCache = new ConcurrentDictionary<string, CharacterCache>();

        //public ConcurrentDictionary<string, CharacterCache> MyCharacterCacheDic { get { return _lstCharacterCache; } }

        private readonly FileSystemWatcher watcherCharacterRosterFolder;
        private bool _blnSkipUpdate;

        public frmCharacterRoster()
        {
            InitializeComponent();
            this.TranslateWinForm();

            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                watcherCharacterRosterFolder = new FileSystemWatcher(GlobalOptions.CharacterRosterPath, "*.chum5");
            }
        }


        public void SetMyEventHandlers(bool deleteThem = false)
        {
            if(!deleteThem)
            {
                GlobalOptions.MRUChanged += PopulateCharacterList;
                treCharacterList.ItemDrag += treCharacterList_OnDefaultItemDrag;
                treCharacterList.DragEnter += treCharacterList_OnDefaultDragEnter;
                treCharacterList.DragDrop += treCharacterList_OnDefaultDragDrop;
                treCharacterList.DragOver += treCharacterList_OnDefaultDragOver;
                OnMyMouseDown += OnDefaultMouseDown;
                if (watcherCharacterRosterFolder != null)
                {
                    watcherCharacterRosterFolder.Changed += RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Created += RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Deleted += RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Renamed += RefreshWatchListOnly;
                }
            }
            else
            {
                GlobalOptions.MRUChanged -= PopulateCharacterList;
                treCharacterList.ItemDrag -= treCharacterList_OnDefaultItemDrag;
                treCharacterList.DragEnter -= treCharacterList_OnDefaultDragEnter;
                treCharacterList.DragDrop -= treCharacterList_OnDefaultDragDrop;
                treCharacterList.DragOver -= treCharacterList_OnDefaultDragOver;
                OnMyMouseDown -= OnDefaultMouseDown;

                if(watcherCharacterRosterFolder != null)
                {
                    watcherCharacterRosterFolder.Changed -= RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Created -= RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Deleted -= RefreshWatchListOnly;
                    watcherCharacterRosterFolder.Renamed -= RefreshWatchListOnly;
                }
            }
        }

        private void frmCharacterRoster_Load(object sender, EventArgs e)
        {
            SetMyEventHandlers();
            LoadCharacters();
            UpdateCharacter(null);
        }

        private void frmCharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetMyEventHandlers(true);
        }

        public void RefreshWatchListOnly(object sender, EventArgs e)
        {
            if(_blnSkipUpdate)
                return;

            SuspendLayout();
            LoadCharacters(false, false, true, false);
            ResumeLayout();
        }

        public void PopulateCharacterList(object sender, TextEventArgs e)
        {
            if(_blnSkipUpdate)
                return;

            SuspendLayout();
            if(e?.Text != "mru")
            {
                try
                {
                    treCharacterList.Nodes.Clear();
                    LoadCharacters(true, true, true, false);
                    GC.Collect();
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
            }
            else
            {
                LoadCharacters(false, true, true, false);
            }
            ResumeLayout();
        }

        public void RefreshNodes()
        {
            foreach(TreeNode objTypeNode in treCharacterList.Nodes)
            {
                foreach(TreeNode objCharacterNode in objTypeNode.Nodes)
                {
                    if (objCharacterNode.Tag is CharacterCache objCache)
                    {
                        objCharacterNode.Text = objCache.CalculatedName();
                        objCharacterNode.ToolTipText = objCache.FilePath.CheapReplace(Utils.GetStartupPath,
                            () => '<' + Application.ProductName + '>');
                        if (!string.IsNullOrEmpty(objCache.ErrorText))
                        {
                            objCharacterNode.ForeColor = Color.Red;
                            objCharacterNode.ToolTipText += Environment.NewLine + Environment.NewLine
                                                                                + LanguageManager.GetString(
                                                                                    "String_Error",
                                                                                    GlobalOptions.Language) +
                                                                                LanguageManager.GetString(
                                                                                    "String_Colon",
                                                                                    GlobalOptions.Language)
                                                                                + Environment.NewLine +
                                                                                objCache.ErrorText;
                        }
                        else
                            objCharacterNode.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }

        public void LoadCharacters(bool blnRefreshFavorites = true, bool blnRefreshRecents = true, bool blnRefreshWatch = true, bool blnRefreshPlugins = true)
        {
            ReadOnlyObservableCollection<string> lstFavorites = new ReadOnlyObservableCollection<string>(GlobalOptions.FavoritedCharacters);
            bool blnAddFavoriteNode = false;
            TreeNode objFavoriteNode = null;
            TreeNode[] lstFavoritesNodes = null;
            if(blnRefreshFavorites)
            {
                objFavoriteNode = treCharacterList.FindNode("Favorite", false);
                if(objFavoriteNode == null)
                {
                    objFavoriteNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_FavoriteCharacters")) { Tag = "Favorite" };
                    blnAddFavoriteNode = true;
                }

                lstFavoritesNodes = new TreeNode[lstFavorites.Count];
            }

            List<string> lstRecents = new List<string>(GlobalOptions.MostRecentlyUsedCharacters);

            Dictionary<string,string> dicWatch = new Dictionary<string, string>();
            int intWatchFolderCount = 0;
            if(!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                intWatchFolderCount++;
                string[] objFiles = Directory.GetFiles(GlobalOptions.CharacterRosterPath, "*.chum5", SearchOption.AllDirectories);
                for(int i = 0; i < objFiles.Length; ++i)
                {
                    string strFile = objFiles[i];
                    // Make sure we're not loading a character that was already loaded by the MRU list.
                    if (lstFavorites.Contains(strFile) ||
                        lstRecents.Contains(strFile))
                        continue;
                    FileInfo objInfo = new FileInfo(strFile);
                    if (objInfo.Directory == null || objInfo.Directory.FullName == GlobalOptions.CharacterRosterPath)
                    {
                        dicWatch.Add(strFile,"Watch");
                        continue;
                    }

                    string strNewParent = objInfo.Directory.FullName.Replace(GlobalOptions.CharacterRosterPath+"\\", string.Empty);
                    dicWatch.Add(strFile,strNewParent);
                }

                intWatchFolderCount++;
            }

            bool blnAddWatchNode = false;
            TreeNode objWatchNode = null;
            TreeNode[] lstWatchNodes = null;
            if(blnRefreshWatch)
            {
                objWatchNode = treCharacterList.FindNode("Watch", false);
                objWatchNode?.Remove();
                blnAddWatchNode = dicWatch.Count > 0;

                if (blnAddWatchNode)
                {
                    objWatchNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_WatchFolder")) { Tag = "Watch" };
                }

                lstWatchNodes = new TreeNode[intWatchFolderCount];
            }

            bool blnAddRecentNode = false;
            TreeNode objRecentNode = null;
            TreeNode[] lstRecentsNodes = null;
            if(blnRefreshRecents)
            {
                // Add any characters that are open to the displayed list so we can have more than 10 characters listed
                foreach(CharacterShared objCharacterForm in Program.MainForm.OpenCharacterForms)
                {
                    string strFile = objCharacterForm.CharacterObject.FileName;
                    // Make sure we're not loading a character that was already loaded by the MRU list.
                    if(lstFavorites.Contains(strFile) ||
                        lstRecents.Contains(strFile) ||
                        dicWatch.ContainsValue(strFile))
                        continue;

                    lstRecents.Add(strFile);
                }

                foreach(string strFavorite in lstFavorites)
                    lstRecents.Remove(strFavorite);

                objRecentNode = treCharacterList.FindNode("Recent", false);
                if(objRecentNode == null && lstRecents.Count > 0)
                {
                    objRecentNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_RecentCharacters")) { Tag = "Recent" };
                    blnAddRecentNode = true;
                }

                lstRecentsNodes = new TreeNode[lstRecents.Count];
            }
            Parallel.Invoke(
                () => {
                    if(objFavoriteNode != null && lstFavoritesNodes != null)
                    {
                        object lstFavoritesNodesLock = new object();

                        Parallel.For(0, lstFavorites.Count, i =>
                        {
                            string strFile = lstFavorites[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock(lstFavoritesNodesLock)
                                lstFavoritesNodes[i] = objNode;
                        });

                        if(blnAddFavoriteNode)
                        {
                            for(int i = 0; i < lstFavoritesNodes.Length; i++)
                            {
                                TreeNode objNode = lstFavoritesNodes[i];
                                if(objNode != null)
                                    objFavoriteNode.Nodes.Add(objNode);
                            }
                        }
                    }
                },
                () => {
                    if(objRecentNode != null && lstRecentsNodes != null)
                    {
                        object lstRecentsNodesLock = new object();

                        Parallel.For(0, lstRecents.Count, i =>
                        {
                            string strFile = lstRecents[i];
                            TreeNode objNode = CacheCharacter(strFile);
                            lock(lstRecentsNodesLock)
                                lstRecentsNodes[i] = objNode;
                        });

                        if(blnAddRecentNode)
                        {
                            for(int i = 0; i < lstRecentsNodes.Length; i++)
                            {
                                TreeNode objNode = lstRecentsNodes[i];
                                if(objNode != null)
                                    objRecentNode.Nodes.Add(objNode);
                            }
                        }
                    }
                },
                () =>
                {
                    if(objWatchNode != null && lstWatchNodes != null)
                    {
                        ConcurrentBag<KeyValuePair<TreeNode,string>> bagNodes = new ConcurrentBag<KeyValuePair<TreeNode, string>>();
                        Parallel.ForEach(dicWatch, i => bagNodes.Add(new KeyValuePair<TreeNode, string>(CacheCharacter(i.Key), i.Value)));

                        if(blnAddWatchNode)
                        {
                            foreach (string s in dicWatch.Values.Distinct())
                            {
                                if (s == "Watch") continue;
                                objWatchNode.Nodes.Add(new TreeNode(s){Tag = s});
                            }

                            foreach (KeyValuePair<TreeNode, string> kvtNode in bagNodes)
                            {
                                if (kvtNode.Value == "Watch")
                                {
                                    objWatchNode.Nodes.Add(kvtNode.Key);
                                }
                                else
                                {
                                    foreach (TreeNode objNode in objWatchNode.Nodes)
                                    {
                                        if (objNode.Tag.ToString() == kvtNode.Value)
                                        {
                                            objNode.Nodes.Add(kvtNode.Key);
                                        }
                                    }
                                }
                            }

                        }
                    }
                },
                () =>
                {
                    foreach(IPlugin plugin in Program.PluginLoader.MyActivePlugins)
                    {
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
                        var t = Task.Factory.StartNew<ICollection<TreeNode>>(() =>
                        {
                            Log.Info("Starting new Task to get CharacterRosterTreeNodes for plugin:" + plugin);
                            var task = plugin.GetCharacterRosterTreeNode(this, blnRefreshPlugins);
                            if(task.Result != null)
                            {
                                return task.Result.OrderBy(a => a.Text).ToList();
                            }
                            return new List<TreeNode>();
                        });
                        t.ContinueWith(nodelist =>
                        {
                            foreach(TreeNode node in nodelist.Result)
                            {
                                TreeNode objExistingNode = treCharacterList.Nodes.Cast<TreeNode>().FirstOrDefault(x => x.Text == node.Text && x.Tag == node.Tag);
                                Program.MainForm.DoThreadSafe(() =>
                                {
                                    try
                                    {
                                        if (objExistingNode != null)
                                        {
                                            treCharacterList.Nodes.Remove(objExistingNode);
                                        }

                                        if (node.Nodes.Count > 0 || !string.IsNullOrEmpty(node.ToolTipText)
                                            || node.Tag != null)
                                        {
                                            if (treCharacterList.IsDisposed)
                                                return;
                                            if (treCharacterList.Nodes.ContainsKey(node.Name))
                                                treCharacterList.Nodes.RemoveByKey(node.Name);
                                            treCharacterList.Nodes.Insert(1, node);
                                        }

                                        node.Expand();
                                    }
                                    catch (ObjectDisposedException e)
                                    {
                                        Log.Trace(e);
                                    }
                                    catch (InvalidAsynchronousStateException e)
                                    {
                                        Log.Trace(e);
                                    }
                                    catch (ArgumentException e)
                                    {
                                        Log.Trace(e);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Warn(e);
                                    }
                                });
                            }
                            Log.Info("Task to get and add CharacterRosterTreeNodes for plugin " + plugin + " finished.");
                        });
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
                    }
                });
            Log.Info("Populating CharacterRosterTreeNode (MainThread).");
            if(objFavoriteNode != null)
            {
                if(blnAddFavoriteNode)
                {
                    treCharacterList.Nodes.Add(objFavoriteNode);
                    objFavoriteNode.Expand();
                }
                else
                {
                    objFavoriteNode.Nodes.Clear();
                    for (int i = 0; i < lstFavoritesNodes.Length; i++)
                    {
                        TreeNode objNode = lstFavoritesNodes[i];
                        if(objNode != null)
                            objFavoriteNode.Nodes.Add(objNode);
                    }
                }
            }

            if(objRecentNode != null)
            {
                if(blnAddRecentNode)
                {
                    treCharacterList.Nodes.Add(objRecentNode);
                    objRecentNode.Expand();
                }
                else
                {
                    try
                    {
                        objRecentNode.Nodes.Clear();
                        for (int i = 0; i < lstRecentsNodes.Length; i++)
                        {
                            TreeNode objNode = lstRecentsNodes[i];
                            if (objNode != null)
                                objRecentNode.Nodes.Add(objNode);
                        }
                    }
                    catch (ObjectDisposedException e)
                    {
                        //just swallow this
                        Log.Trace(e, "ObjectDisposedException can be ignored here.");
                    }
                }
            }
            if(objWatchNode != null)
            {
                if(blnAddWatchNode)
                {
                    treCharacterList.Nodes.Add(objWatchNode);
                    objWatchNode.Expand();
                }
                else
                {
                    objWatchNode.Nodes.Clear();
                    for(int i = 0; i < lstWatchNodes.Length; i++)
                    {
                        TreeNode objNode = lstWatchNodes[i];
                        if(objNode != null)
                            objWatchNode.Nodes.Add(objNode);
                    }
                }
            }
            treCharacterList.ExpandAll();
            UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache);
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// </summary>
        /// <param name="strFile"></param>
        private static TreeNode CacheCharacter(string strFile)
        {
            CharacterCache objCache = new CharacterCache(strFile);
            TreeNode objNode = new TreeNode
            {
                Text = objCache.CalculatedName(),
                ToolTipText = objCache.FilePath.CheapReplace(Utils.GetStartupPath, () => '<' + Application.ProductName + '>'),
                Tag = objCache
            };
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                objNode.ForeColor = Color.Red;
                objNode.ToolTipText += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Error")
                                       + LanguageManager.GetString("String_Colon") + Environment.NewLine + objCache.ErrorText;
            }
            return objNode;
        }



        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        public void UpdateCharacter(CharacterCache objCache)
        {
            if (IsDisposed) // Safety check for external calls
                return;
            if(objCache != null)
            {
                string strUnknown = LanguageManager.GetString("String_Unknown");
                string strNone = LanguageManager.GetString("String_None");
                if (objCache.Description.IsRtf())
                    rtbCharacterBio.Rtf = objCache.Description;
                else
                    rtbCharacterBio.Text = objCache.Description;
                if (objCache.Background.IsRtf())
                    rtbCharacterBackground.Rtf = objCache.Background;
                else
                    rtbCharacterBackground.Text = objCache.Background;
                if (objCache.CharacterNotes.IsRtf())
                    rtbCharacterNotes.Rtf = objCache.CharacterNotes;
                else
                    rtbCharacterNotes.Text = objCache.CharacterNotes;
                if (objCache.GameNotes.IsRtf())
                    rtbGameNotes.Rtf = objCache.GameNotes;
                else
                    rtbGameNotes.Text = objCache.GameNotes;
                if (objCache.Concept.IsRtf())
                    rtbCharacterConcept.Rtf = objCache.Concept;
                else
                    rtbCharacterConcept.Text = objCache.Concept;
                lblCareerKarma.Text = objCache.Karma;
                if(string.IsNullOrEmpty(lblCareerKarma.Text) || lblCareerKarma.Text == 0.ToString(GlobalOptions.CultureInfo))
                    lblCareerKarma.Text = strNone;
                lblPlayerName.Text = objCache.PlayerName;
                if(string.IsNullOrEmpty(lblPlayerName.Text))
                    lblPlayerName.Text = strUnknown;
                lblCharacterName.Text = objCache.CharacterName;
                if(string.IsNullOrEmpty(lblCharacterName.Text))
                    lblCharacterName.Text = strUnknown;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                if(string.IsNullOrEmpty(lblCharacterAlias.Text))
                    lblCharacterAlias.Text = strUnknown;
                lblEssence.Text = objCache.Essence;
                if(string.IsNullOrEmpty(lblEssence.Text))
                    lblEssence.Text = strUnknown;
                lblFilePath.Text = objCache.FileName;
                if(string.IsNullOrEmpty(lblFilePath.Text))
                    lblFilePath.Text = LanguageManager.GetString("MessageTitle_FileNotFound");
                lblSettings.Text = objCache.SettingsFile;
                if(string.IsNullOrEmpty(lblSettings.Text))
                    lblSettings.Text = strUnknown;
                lblFilePath.SetToolTip(objCache.FilePath.CheapReplace(Utils.GetStartupPath, () => '<' + Application.ProductName + '>'));
                picMugshot.Image?.Dispose();
                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml");
                if (objCache.Metatype != null)
                {
                    XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + "]");
                    if (objMetatypeNode == null)
                    {
                        objMetatypeDoc = XmlManager.Load("critters.xml");
                        objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + "]");
                    }

                    string strMetatype = objMetatypeNode?["translate"]?.InnerText ?? objCache.Metatype;

                    if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                    {
                        objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + "]");

                        strMetatype += LanguageManager.GetString("String_Space") + '(' + (objMetatypeNode?["translate"]?.InnerText ?? objCache.Metavariant) + ')';
                    }
                    lblMetatype.Text = strMetatype;
                }
                else
                    lblMetatype.Text = LanguageManager.GetString("String_MetatypeLoadError");
                tabCharacterText.Visible = true;
                if (!string.IsNullOrEmpty(objCache.ErrorText))
                {
                    rtbCharacterBio.Text = objCache.ErrorText;
                    rtbCharacterBio.ForeColor = Color.Red;
                    rtbCharacterBio.BringToFront();
                }
                else
                    rtbCharacterBio.ForeColor = SystemColors.WindowText;
            }
            else
            {
                tabCharacterText.Visible = false;
                rtbCharacterBio.Clear();
                rtbCharacterBackground.Clear();
                rtbCharacterNotes.Clear();
                rtbGameNotes.Clear();
                rtbCharacterConcept.Clear();
                lblCareerKarma.Text = string.Empty;
                lblMetatype.Text = string.Empty;
                lblPlayerName.Text = string.Empty;
                lblCharacterName.Text = string.Empty;
                lblCharacterAlias.Text = string.Empty;
                lblEssence.Text = string.Empty;
                lblFilePath.Text = string.Empty;
                lblFilePath.SetToolTip(string.Empty);
                lblSettings.Text = string.Empty;
                picMugshot.Image = null;
            }
            lblCareerKarmaLabel.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text);
            lblMetatypeLabel.Visible = !string.IsNullOrEmpty(lblMetatype.Text);
            lblPlayerNameLabel.Visible = !string.IsNullOrEmpty(lblPlayerName.Text);
            lblCharacterNameLabel.Visible = !string.IsNullOrEmpty(lblCharacterName.Text);
            lblCharacterAliasLabel.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text);
            lblEssenceLabel.Visible = !string.IsNullOrEmpty(lblEssence.Text);
            lblFilePathLabel.Visible = !string.IsNullOrEmpty(lblFilePath.Text);
            lblSettingsLabel.Visible = !string.IsNullOrEmpty(lblSettings.Text);
            ProcessMugshotSizeMode();
        }

        #region Form Methods

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null)
                return;
            CharacterCache objCache = objSelectedNode.Tag as CharacterCache;
            objCache?.OnMyAfterSelect(sender, e);
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if(objSelectedNode != null && objSelectedNode.Level > 0)
            {
                if (objSelectedNode.Tag == null) return;
                if(objSelectedNode.Tag is CharacterCache objCache)
                {
                    try
                    {
                        objCache.OnMyDoubleClick(sender, e);
                        objSelectedNode.Text = objCache.CalculatedName();
                        Cursor = Cursors.WaitCursor;
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }

                }
            }
        }
        private void treCharacterList_OnDefaultKeyDown(object sender, KeyEventArgs e)
        {

            TreeNode t = treCharacterList.SelectedNode;

            var objCache = t?.Tag as CharacterCache;
            objCache?.OnMyKeyDown(sender, new Tuple<KeyEventArgs, TreeNode>(e, t));
        }

        private void treCharacterList_OnDefaultDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treCharacterList_OnDefaultDragOver(object sender, DragEventArgs e)
        {
            if(!(sender is TreeView treSenderView))
                return;
            Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = treSenderView.GetNodeAt(pt);
            if(objNode != null)
            {
                if(objNode.Parent != null)
                    objNode = objNode.Parent;
                if(objNode.Tag?.ToString() != "Watch")
                {
                    objNode.BackColor = SystemColors.ControlDark;
                }
            }

            // Clear the background colour for all other Nodes.
            treCharacterList.ClearNodeBackground(objNode);
        }

        private void treCharacterList_OnDefaultDragDrop(object sender, DragEventArgs e)
        {
            // Do not allow the root element to be moved.
            if(treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 || treCharacterList.SelectedNode.Parent?.Tag?.ToString() == "Watch")
                return;

            if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                if(!(sender is TreeView treSenderView))
                    return;
                Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                if (nodDestinationNode?.Level > 0)
                    nodDestinationNode = nodDestinationNode.Parent;
                string strDestinationNode = nodDestinationNode?.Tag?.ToString();
                if(strDestinationNode != "Watch")
                {
                    if(!(e.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                        return;

                    if(nodNewNode.Level == 0 || nodNewNode.Parent == nodDestinationNode)
                        return;
                    if (nodNewNode.Tag is CharacterCache objCache)
                    {
                        switch(strDestinationNode)
                        {
                            case "Recent":
                                GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                                GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                                break;
                            case "Favorite":
                                GlobalOptions.FavoritedCharacters.Add(objCache.FilePath);
                                break;
                        }
                    }
                }

                IPlugin plugintag = null;
                while (nodDestinationNode?.Tag != null && plugintag == null)
                {
                    if (nodDestinationNode.Tag is IPlugin temp)
                        plugintag = temp;
                    nodDestinationNode = nodDestinationNode.Parent;
                }
                plugintag?.DoCharacterList_DragDrop(sender, e, treCharacterList);
            }
        }

        private void treCharacterList_OnDefaultItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            ProcessMugshotSizeMode();
        }

        private void ProcessMugshotSizeMode()
        {
            picMugshot.SizeMode = picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width
                ? PictureBoxSizeMode.CenterImage
                : PictureBoxSizeMode.Zoom;
        }
        #endregion

        public void tsDelete_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                objCache.OnMyContextMenuDeleteClick(t, e);
            }
        }

        private void tsSort_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.MostRecentlyUsedCharacters.Sort();
                        break;
                    case "Favorite":
                        GlobalOptions.FavoritedCharacters.Sort();
                        break;
                }
            }
            else if (t?.Tag != null)
            {
                switch (t.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.MostRecentlyUsedCharacters.Sort();
                        break;
                    case "Favorite":
                        GlobalOptions.FavoritedCharacters.Sort();
                        break;
                }
            }
            treCharacterList.SelectedNode = t;
        }

        private void tsToggleFav_Click(object sender, EventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;

            if(t?.Tag is CharacterCache objCache)
            {
                switch(t.Parent.Tag.ToString())
                {
                    case "Favorite":
                        GlobalOptions.FavoritedCharacters.Remove(objCache.FilePath);
                        GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                        break;
                    default:
                        GlobalOptions.FavoritedCharacters.Add(objCache.FilePath);
                        break;
                }
                treCharacterList.SelectedNode = t;
            }
        }


        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<MouseEventArgs> OnMyMouseDown;

        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            OnMyMouseDown?.Invoke(sender, e);
        }

        public void OnDefaultMouseDown(object sender, MouseEventArgs e)
        {
            if (sender is TreeView objTree && e != null)
            {
                // Generic event for all TreeViews to allow right-clicking to select a TreeNode so the proper ContextMenu is shown.
                //if (e.Button == System.Windows.Forms.MouseButtons.Right)
                //{
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
        }

        private void tsCloseOpenCharacter_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if(objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            string strFile = objSelectedNode.Tag.ToString();
            if(string.IsNullOrEmpty(strFile))
                return;
            Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
            Cursor = Cursors.WaitCursor;
            if(objOpenCharacter != null)
            {
                Program.MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objOpenCharacter)?.Close();
            }
            Cursor = Cursors.Default;
        }

        private void TreCharacterList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treCharacterList.SelectedNode = e.Node;
            }
            if (e.Node.Tag != null)
            {
                string strTag = e.Node.Tag.ToString();
                if (!String.IsNullOrEmpty(strTag))
                    e.Node.ContextMenuStrip = CreateContextMenuStrip(
                        strTag.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                        && Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject?.FileName == strTag));
                else
                    e.Node.ContextMenuStrip = CreateContextMenuStrip(false);
            }
            foreach (var plugin in Program.PluginLoader.MyActivePlugins)
            {
                plugin.SetCharacterRosterNode(e.Node);
            }
        }

        public ContextMenuStrip CreateContextMenuStrip(bool blnIncludeCloseOpenCharacter)
        {
            // 
            // tsToggleFav
            //
            ToolStripMenuItem tsToggleFav = new ToolStripMenuItem
            {
                Image = Properties.Resources.asterisk_orange,
                Name = "tsToggleFav",
                Size = new Size(180, 22),
                Tag = "Menu_ToggleFavorite"
            };
            tsToggleFav.Click += tsToggleFav_Click;
            // 
            // tsSort
            //
            ToolStripMenuItem tsSort = new ToolStripMenuItem
            {
                Image = Properties.Resources.page_refresh,
                Name = "tsSort",
                Size = new Size(180, 22),
                Tag = "Menu_Sort"
            };
            tsSort.Click += tsSort_Click;
            // 
            // tsDelete
            //
            ToolStripMenuItem tsDelete = new ToolStripMenuItem
            {
                Image = Properties.Resources.delete,
                Name = "tsDelete",
                Size = new Size(180, 22),
                Tag = "Menu_Delete"
            };
            tsDelete.Click += tsDelete_Click;
            // 
            // cmsRoster
            //
            ContextMenuStrip cmsRoster = new ContextMenuStrip
            {
                Name = "cmsRoster",
                Size = new Size(181, 114)
            };
            cmsRoster.Items.AddRange(new ToolStripItem[]
            {
                tsToggleFav,
                tsSort,
                tsDelete
            });

            tsToggleFav.TranslateToolStripItemsRecursively();
            tsSort.TranslateToolStripItemsRecursively();
            tsDelete.TranslateToolStripItemsRecursively();

            if (blnIncludeCloseOpenCharacter)
            {
                // 
                // tsCloseOpenCharacter
                //
                ToolStripMenuItem tsCloseOpenCharacter = new ToolStripMenuItem
                {
                    Image = Properties.Resources.door_out,
                    Name = "tsCloseOpenCharacter",
                    Size = new Size(180, 22),
                    Tag = "Menu_Close"
                };
                tsCloseOpenCharacter.Click += tsCloseOpenCharacter_Click;
                cmsRoster.Items.Add(tsCloseOpenCharacter);
                tsCloseOpenCharacter.TranslateToolStripItemsRecursively();
            }

            return cmsRoster;
        }
    }
}
