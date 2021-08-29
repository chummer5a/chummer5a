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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.XPath;
using Chummer.Plugins;
using Newtonsoft.Json;
using NLog;

namespace Chummer
{
    public partial class frmCharacterRoster : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        private readonly FileSystemWatcher _watcherCharacterRosterFolder;
        private Task _tskMostRecentlyUsedsRefresh;
        private Task _tskWatchFolderRefresh;
        private CancellationTokenSource _objMostRecentlyUsedsRefreshCancellationTokenSource;
        private CancellationTokenSource _objWatchFolderRefreshCancellationTokenSource;

        public frmCharacterRoster()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                _watcherCharacterRosterFolder = new FileSystemWatcher(GlobalOptions.CharacterRosterPath, "*.chum5");
            }
        }

        public void SetMyEventHandlers(bool deleteThem = false)
        {
            if (!deleteThem)
            {
                GlobalOptions.MruChanged += RefreshMruLists;
                treCharacterList.ItemDrag += treCharacterList_OnDefaultItemDrag;
                treCharacterList.DragEnter += treCharacterList_OnDefaultDragEnter;
                treCharacterList.DragDrop += treCharacterList_OnDefaultDragDrop;
                treCharacterList.DragOver += treCharacterList_OnDefaultDragOver;
                treCharacterList.NodeMouseDoubleClick += treCharacterList_OnDefaultDoubleClick;
                OnMyMouseDown += OnDefaultMouseDown;
                if (_watcherCharacterRosterFolder != null)
                {
                    _watcherCharacterRosterFolder.Changed += RefreshWatchList;
                    _watcherCharacterRosterFolder.Created += RefreshWatchList;
                    _watcherCharacterRosterFolder.Deleted += RefreshWatchList;
                    _watcherCharacterRosterFolder.Renamed += RefreshWatchList;
                }
            }
            else
            {
                GlobalOptions.MruChanged -= RefreshMruLists;
                treCharacterList.ItemDrag -= treCharacterList_OnDefaultItemDrag;
                treCharacterList.DragEnter -= treCharacterList_OnDefaultDragEnter;
                treCharacterList.DragDrop -= treCharacterList_OnDefaultDragDrop;
                treCharacterList.DragOver -= treCharacterList_OnDefaultDragOver;
                treCharacterList.NodeMouseDoubleClick -= treCharacterList_OnDefaultDoubleClick;
                OnMyMouseDown = null;

                if (_watcherCharacterRosterFolder != null)
                {
                    _watcherCharacterRosterFolder.Changed -= RefreshWatchList;
                    _watcherCharacterRosterFolder.Created -= RefreshWatchList;
                    _watcherCharacterRosterFolder.Deleted -= RefreshWatchList;
                    _watcherCharacterRosterFolder.Renamed -= RefreshWatchList;
                }
            }
        }

        private async void frmCharacterRoster_Load(object sender, EventArgs e)
        {
            SetMyEventHandlers();
            _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
            _tskMostRecentlyUsedsRefresh = LoadMruCharacters(true);
            _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
            _tskWatchFolderRefresh = LoadWatchFolderCharacters();
            try
            {
                await Task.WhenAll(_tskMostRecentlyUsedsRefresh, _tskWatchFolderRefresh,
                    Task.WhenAll(Program.PluginLoader.MyActivePlugins.Select(RefreshPluginNodes)));
            }
            catch (TaskCanceledException) { }
            UpdateCharacter(null);
        }

        private void frmCharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objMostRecentlyUsedsRefreshCancellationTokenSource?.Cancel(false);
            _objWatchFolderRefreshCancellationTokenSource?.Cancel(false);
            SetMyEventHandlers(true);
        }

        private async void RefreshWatchList(object sender, EventArgs e)
        {
            _objWatchFolderRefreshCancellationTokenSource?.Cancel(false);
            _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskWatchFolderRefresh?.IsCompleted == false)
                    await _tskWatchFolderRefresh;
            }
            catch (TaskCanceledException) { }
            SuspendLayout();
            _tskWatchFolderRefresh = LoadWatchFolderCharacters();
            try
            {
                await _tskWatchFolderRefresh;
            }
            catch (ObjectDisposedException)
            {
                //swallow this
            }
            catch (TaskCanceledException) { }
            ResumeLayout();
        }

        private async void RefreshMruLists(object sender, TextEventArgs e)
        {
            await RefreshMruLists(e?.Text);
        }

        public async Task RefreshMruLists(string strMruType)
        {
            _objMostRecentlyUsedsRefreshCancellationTokenSource?.Cancel(false);
            _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskMostRecentlyUsedsRefresh?.IsCompleted == false)
                    await _tskMostRecentlyUsedsRefresh;
            }
            catch (TaskCanceledException) { }
            SuspendLayout();
            _tskMostRecentlyUsedsRefresh = LoadMruCharacters(strMruType != "mru");
            try
            {
                await _tskMostRecentlyUsedsRefresh;
            }
            catch (ObjectDisposedException)
            {
                //swallow this
            }
            catch (TaskCanceledException) { }
            ResumeLayout();
        }

        public void RefreshNodeTexts()
        {
            foreach (TreeNode objCharacterNode in treCharacterList.Nodes.Cast<TreeNode>().GetAllDescendants(x => x.Nodes.Cast<TreeNode>()))
            {
                if (!(objCharacterNode.Tag is CharacterCache objCache))
                    continue;
                string strCalculatedName = objCache.CalculatedName();
                treCharacterList.QueueThreadSafe(() => objCharacterNode.Text = strCalculatedName);
                string strTooltip = string.Empty;
                if (!string.IsNullOrEmpty(objCache.FilePath))
                    strTooltip = objCache.FilePath.Replace(Utils.GetStartupPath, '<' + Application.ProductName + '>');
                if (!string.IsNullOrEmpty(objCache.ErrorText))
                {
                    treCharacterList.QueueThreadSafe(() => objCharacterNode.ForeColor = ColorManager.ErrorColor);
                    if (!string.IsNullOrEmpty(objCache.FilePath))
                        strTooltip += Environment.NewLine + Environment.NewLine;
                    strTooltip += LanguageManager.GetString("String_Error") + LanguageManager.GetString("String_Colon") + Environment.NewLine + objCache.ErrorText;
                }
                else
                    treCharacterList.QueueThreadSafe(() => objCharacterNode.ForeColor = ColorManager.WindowText);
                treCharacterList.QueueThreadSafe(() => objCharacterNode.ToolTipText = strTooltip);
            }
        }

        private async Task LoadMruCharacters(bool blnRefreshFavorites)
        {
            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            List<string> lstFavorites = blnRefreshFavorites ? GlobalOptions.FavoritedCharacters.ToList() : new List<string>();
            bool blnAddFavoriteNode = false;
            TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
            if (objFavoriteNode == null && blnRefreshFavorites)
            {
                objFavoriteNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_FavoriteCharacters"))
                    {Tag = "Favorite"};
                blnAddFavoriteNode = true;
            }
            TreeNode[] lstFavoritesNodes = lstFavorites.Count > 0 ? new TreeNode[lstFavorites.Count] : null;

            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            bool blnAddRecentNode = false;
            List<string> lstRecents = new List<string>(GlobalOptions.MostRecentlyUsedCharacters);
            // Add any characters that are open to the displayed list so we can have more than 10 characters listed
            foreach (CharacterShared objCharacterForm in Program.MainForm.OpenCharacterForms)
            {
                string strFile = objCharacterForm.CharacterObject.FileName;
                // Make sure we're not loading a character that was already loaded by the MRU list.
                if (lstFavorites.Contains(strFile) || lstRecents.Contains(strFile))
                    continue;
                lstRecents.Add(strFile);
            }
            foreach (string strFavorite in lstFavorites)
                lstRecents.Remove(strFavorite);
            TreeNode objRecentNode = treCharacterList.FindNode("Recent", false);
            if (objRecentNode == null && lstRecents.Count > 0)
            {
                objRecentNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_RecentCharacters"))
                    {Tag = "Recent"};
                blnAddRecentNode = true;
            }
            TreeNode[] lstRecentsNodes = lstRecents.Count > 0 ? new TreeNode[lstRecents.Count] : null;

            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            try
            {
                await Task.WhenAll(
                    Task.Run(() =>
                    {
                        if (lstFavoritesNodes == null || lstFavorites.Count <= 0 ||
                            _objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            return;
                        Parallel.For(0, lstFavorites.Count, (i, objState) =>
                        {
                            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            {
                                objState.Stop();
                                return;
                            }

                            if (objState.ShouldExitCurrentIteration)
                                return;
                            lstFavoritesNodes[i] = CacheCharacter(lstFavorites[i]);
                        });
                        if (!blnAddFavoriteNode || objFavoriteNode == null ||
                            _objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            return;
                        foreach (TreeNode objNode in lstFavoritesNodes)
                        {
                            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                                return;
                            if (objNode == null)
                                continue;
                            if (objFavoriteNode.TreeView != null)
                                objFavoriteNode.TreeView.DoThreadSafe(() => objFavoriteNode.Nodes.Add(objNode));
                            else
                                objFavoriteNode.Nodes.Add(objNode);
                        }
                    }, _objMostRecentlyUsedsRefreshCancellationTokenSource.Token),
                    Task.Run(() =>
                    {
                        if (lstRecentsNodes == null || lstRecents.Count <= 0 ||
                            _objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            return;
                        Parallel.For(0, lstRecents.Count, (i, objState) =>
                        {
                            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            {
                                objState.Stop();
                                return;
                            }

                            if (objState.ShouldExitCurrentIteration)
                                return;
                            lstRecentsNodes[i] = CacheCharacter(lstRecents[i]);
                        });
                        if (!blnAddRecentNode || objRecentNode == null ||
                            _objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                            return;
                        foreach (TreeNode objNode in lstRecentsNodes)
                        {
                            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                                return;
                            if (objNode == null)
                                continue;
                            if (objRecentNode.TreeView != null)
                                objRecentNode.TreeView.DoThreadSafe(() => objRecentNode.Nodes.Add(objNode));
                            else
                                objRecentNode.Nodes.Add(objNode);
                        }
                    }, _objMostRecentlyUsedsRefreshCancellationTokenSource.Token));
            }
            catch (TaskCanceledException) { }

            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            Log.Info("Populating CharacterRosterTreeNode MRUs (MainThread).");
            await treCharacterList.DoThreadSafeAsync(() =>
            {
                treCharacterList.SuspendLayout();
                if (blnRefreshFavorites && objFavoriteNode != null)
                {
                    if (blnAddFavoriteNode)
                    {
                        treCharacterList.Nodes.Insert(0, objFavoriteNode);
                    }
                    else if (lstFavoritesNodes != null)
                    {
                        objFavoriteNode.Nodes.Clear();
                        foreach (TreeNode objNode in lstFavoritesNodes)
                        {
                            if (objNode != null)
                                objFavoriteNode.Nodes.Add(objNode);
                        }
                    }
                    objFavoriteNode.ExpandAll();
                }

                if (objRecentNode != null)
                {
                    if (blnAddRecentNode)
                    {
                        treCharacterList.Nodes.Insert(objFavoriteNode != null ? 1 : 0, objRecentNode);
                    }
                    else if (lstRecentsNodes != null)
                    {
                        try
                        {
                            objRecentNode.Nodes.Clear();
                            foreach (TreeNode objNode in lstRecentsNodes)
                            {
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
                    objRecentNode.ExpandAll();
                }
                treCharacterList.ResumeLayout();
            });

            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            await this.DoThreadSafeAsync(() => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache));
        }

        private async Task LoadWatchFolderCharacters()
        {
            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            Dictionary<string, string> dicWatch = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(GlobalOptions.CharacterRosterPath) && Directory.Exists(GlobalOptions.CharacterRosterPath))
            {
                foreach (string strFile in Directory.GetFiles(GlobalOptions.CharacterRosterPath, "*.chum5", SearchOption.AllDirectories))
                {
                    if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                        return;

                    FileInfo objInfo = new FileInfo(strFile);
                    if (objInfo.Directory == null || objInfo.Directory.FullName == GlobalOptions.CharacterRosterPath)
                    {
                        dicWatch.Add(strFile, "Watch");
                        continue;
                    }

                    string strNewParent = objInfo.Directory.FullName.Replace(GlobalOptions.CharacterRosterPath + "\\", string.Empty);
                    dicWatch.Add(strFile, strNewParent);
                }
            }
            bool blnAddWatchNode = dicWatch.Count > 0;
            TreeNode objWatchNode = treCharacterList.FindNode("Watch", false);
            if (blnAddWatchNode)
            {
                if (objWatchNode != null)
                    objWatchNode.Nodes.Clear();
                else
                    objWatchNode = new TreeNode(LanguageManager.GetString("Treenode_Roster_WatchFolder")) { Tag = "Watch" };
            }
            else
                objWatchNode?.Remove();

            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            await Task.Run(() =>
            {
                if (objWatchNode == null || !blnAddWatchNode || dicWatch.Count <= 0 || _objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                    return;
                ConcurrentDictionary<TreeNode, string> dicWatchNodes = new ConcurrentDictionary<TreeNode, string>();
                Parallel.ForEach(dicWatch, (kvpLoop, objState) =>
                {
                    if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                    {
                        objState.Stop();
                        return;
                    }
                    if (objState.ShouldExitCurrentIteration)
                        return;
                    dicWatchNodes.TryAdd(CacheCharacter(kvpLoop.Key), kvpLoop.Value);
                });
                foreach (string s in dicWatchNodes.Values.Distinct())
                {
                    if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                        return;
                    if (s == "Watch")
                        continue;
                    if (objWatchNode.TreeView != null)
                        objWatchNode.TreeView.DoThreadSafe(() => objWatchNode.Nodes.Add(new TreeNode(s) { Tag = s }));
                    else
                        objWatchNode.Nodes.Add(new TreeNode(s) { Tag = s });
                }

                foreach (KeyValuePair<TreeNode, string> kvtNode in dicWatchNodes)
                {
                    if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                        return;
                    if (kvtNode.Value == "Watch")
                    {
                        if (objWatchNode.TreeView != null)
                            objWatchNode.TreeView.DoThreadSafe(() => objWatchNode.Nodes.Add(kvtNode.Key));
                        else
                            objWatchNode.Nodes.Add(kvtNode.Key);
                    }
                    else
                    {
                        foreach (TreeNode objNode in objWatchNode.Nodes)
                        {
                            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                                return;
                            if (objNode.Tag.ToString() == kvtNode.Value)
                            {
                                if (objWatchNode.TreeView != null)
                                    objWatchNode.TreeView.DoThreadSafe(() => objWatchNode.Nodes.Add(kvtNode.Key));
                                else
                                    objNode.Nodes.Add(kvtNode.Key);
                            }
                        }
                    }
                }
            });

            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            Log.Info("Populating CharacterRosterTreeNode Watch Folder (MainThread).");
            await treCharacterList.DoThreadSafeAsync(() =>
            {
                treCharacterList.SuspendLayout();
                if (objWatchNode != null && blnAddWatchNode)
                {
                    if (objWatchNode.TreeView == null)
                    {
                        TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
                        TreeNode objRecentNode = treCharacterList.FindNode("Recent", false);
                        if (objFavoriteNode != null && objRecentNode != null)
                            treCharacterList.Nodes.Insert(2, objWatchNode);
                        else if (objFavoriteNode != null || objRecentNode != null)
                            treCharacterList.Nodes.Insert(1, objWatchNode);
                        else
                            treCharacterList.Nodes.Insert(0, objWatchNode);
                    }
                    objWatchNode.ExpandAll();
                }
                treCharacterList.ResumeLayout();
            });

            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            await this.DoThreadSafeAsync(() => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache));
        }

        public async Task RefreshPluginNodes(IPlugin objPluginToRefresh)
        {
            if (objPluginToRefresh == null)
                throw new ArgumentNullException(nameof(objPluginToRefresh));
            int intNodeOffset = Program.PluginLoader.MyActivePlugins.IndexOf(objPluginToRefresh);
            if (intNodeOffset < 0)
            {
                Utils.BreakIfDebug();
                return;
            }
            await Task.Run(async () =>
            {
                Log.Info("Starting new Task to get CharacterRosterTreeNodes for plugin:" + objPluginToRefresh);
                List<TreeNode> lstNodes =
                    (await objPluginToRefresh.GetCharacterRosterTreeNode(this, true))?.OrderBy(a => a.Text)
                    .ToList() ?? new List<TreeNode>();
                for (int i = 0; i < lstNodes.Count; ++i)
                {
                    TreeNode node = lstNodes[i];
                    TreeNode objExistingNode = await treCharacterList.DoThreadSafeFuncAsync(() =>
                        treCharacterList.Nodes.Cast<TreeNode>()
                            .FirstOrDefault(x => x.Text == node.Text && x.Tag == node.Tag));
                    try
                    {
                        await treCharacterList.DoThreadSafeAsync(() =>
                        {
                            if (objExistingNode != null)
                            {
                                treCharacterList.Nodes.Remove(objExistingNode);
                            }

                            if (node.Nodes.Count > 0 || !string.IsNullOrEmpty(node.ToolTipText) || node.Tag != null)
                            {
                                if (treCharacterList.Nodes.ContainsKey(node.Name))
                                    treCharacterList.Nodes.RemoveByKey(node.Name);
                                TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
                                TreeNode objRecentNode = treCharacterList.FindNode("Recent", false);
                                TreeNode objWatchNode = treCharacterList.FindNode("Watch", false);
                                if (objFavoriteNode != null && objRecentNode != null && objWatchNode != null)
                                    treCharacterList.Nodes.Insert(i + intNodeOffset + 3, node);
                                else if (objFavoriteNode != null || objRecentNode != null || objWatchNode != null)
                                {
                                    if ((objFavoriteNode != null && objRecentNode != null) ||
                                        (objFavoriteNode != null && objWatchNode != null) ||
                                        (objRecentNode != null && objWatchNode != null))
                                        treCharacterList.Nodes.Insert(i + intNodeOffset + 2, node);
                                    else
                                        treCharacterList.Nodes.Insert(i + intNodeOffset + 1, node);
                                }
                                else
                                    treCharacterList.Nodes.Insert(i + intNodeOffset, node);
                            }

                            node.Expand();
                        });
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
                }

                Log.Info("Task to get and add CharacterRosterTreeNodes for plugin " + objPluginToRefresh + " finished.");
            });
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
                ToolTipText = objCache.FilePath.CheapReplace(Utils.GetStartupPath,
                    () => '<' + Application.ProductName + '>'),
                Tag = objCache
            };
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                objNode.ForeColor = ColorManager.ErrorColor;
                if (!string.IsNullOrEmpty(objNode.ToolTipText))
                    objNode.ToolTipText += Environment.NewLine + Environment.NewLine;
                objNode.ToolTipText += LanguageManager.GetString("String_Error") +
                                       LanguageManager.GetString("String_Colon") + Environment.NewLine +
                                       objCache.ErrorText;
            }

            return objNode;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        public void UpdateCharacter(CharacterCache objCache)
        {
            if (this.IsNullOrDisposed()) // Safety check for external calls
                return;
            tlpRight.SuspendLayout();
            if (objCache != null)
            {
                string strUnknown = LanguageManager.GetString("String_Unknown");
                string strNone = LanguageManager.GetString("String_None");
                txtCharacterBio.Text = objCache.Description.RtfToPlainText();
                txtCharacterBackground.Text = objCache.Background.RtfToPlainText();
                txtCharacterNotes.Text = objCache.CharacterNotes.RtfToPlainText();
                txtGameNotes.Text = objCache.GameNotes.RtfToPlainText();
                txtCharacterConcept.Text = objCache.Concept.RtfToPlainText();
                lblCareerKarma.Text = objCache.Karma;
                if (string.IsNullOrEmpty(lblCareerKarma.Text) || lblCareerKarma.Text == 0.ToString(GlobalOptions.CultureInfo))
                    lblCareerKarma.Text = strNone;
                lblPlayerName.Text = objCache.PlayerName;
                if (string.IsNullOrEmpty(lblPlayerName.Text))
                    lblPlayerName.Text = strUnknown;
                lblCharacterName.Text = objCache.CharacterName;
                if (string.IsNullOrEmpty(lblCharacterName.Text))
                    lblCharacterName.Text = strUnknown;
                lblCharacterAlias.Text = objCache.CharacterAlias;
                if (string.IsNullOrEmpty(lblCharacterAlias.Text))
                    lblCharacterAlias.Text = strUnknown;
                lblEssence.Text = objCache.Essence;
                if (string.IsNullOrEmpty(lblEssence.Text))
                    lblEssence.Text = strUnknown;
                lblFilePath.Text = objCache.FileName;
                if (string.IsNullOrEmpty(lblFilePath.Text))
                    lblFilePath.Text = LanguageManager.GetString("MessageTitle_FileNotFound");
                lblSettings.Text = objCache.SettingsFile;
                if (string.IsNullOrEmpty(lblSettings.Text))
                    lblSettings.Text = strUnknown;
                lblFilePath.SetToolTip(objCache.FilePath.CheapReplace(Utils.GetStartupPath, () => '<' + Application.ProductName + '>'));
                picMugshot.Image?.Dispose();
                picMugshot.Image = objCache.Mugshot;

                // Populate character information fields.
                XPathNavigator objMetatypeDoc = XmlManager.LoadXPath("metatypes.xml");
                if (objCache.Metatype != null)
                {
                    XPathNavigator objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + "]");
                    if (objMetatypeNode == null)
                    {
                        objMetatypeDoc = XmlManager.LoadXPath("critters.xml");
                        objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + "]");
                    }

                    string strMetatype = objMetatypeNode?.SelectSingleNode("translate")?.Value ?? objCache.Metatype;

                    if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                    {
                        objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + "]");

                        strMetatype += LanguageManager.GetString("String_Space") + '(' + (objMetatypeNode?.SelectSingleNode("translate")?.Value ?? objCache.Metavariant) + ')';
                    }
                    lblMetatype.Text = strMetatype;
                }
                else
                    lblMetatype.Text = LanguageManager.GetString("String_MetatypeLoadError");
                tabCharacterText.Visible = true;
                if (!string.IsNullOrEmpty(objCache.ErrorText))
                {
                    txtCharacterBio.Text = objCache.ErrorText;
                    txtCharacterBio.ForeColor = ColorManager.ErrorColor;
                    txtCharacterBio.BringToFront();
                }
                else
                    txtCharacterBio.ForeColor = ColorManager.WindowText;
            }
            else
            {
                tabCharacterText.Visible = false;
                txtCharacterBio.Clear();
                txtCharacterBackground.Clear();
                txtCharacterNotes.Clear();
                txtGameNotes.Clear();
                txtCharacterConcept.Clear();
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
            tlpRight.ResumeLayout();
        }

        #region Form Methods

        private void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null)
                return;
            CharacterCache objCache = objSelectedNode.Tag as CharacterCache;
            objCache?.OnMyAfterSelect?.Invoke(sender, e);
            UpdateCharacter(objCache);
            treCharacterList.ClearNodeBackground(treCharacterList.SelectedNode);
        }

        private void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            switch (objSelectedNode.Tag)
            {
                case null:
                    return;

                case CharacterCache objCache:
                    {
                        using (new CursorWait(this))
                        {
                            objCache.OnMyDoubleClick(sender, e);
                        }
                        break;
                    }
            }
        }

        private void treCharacterList_OnDefaultKeyDown(object sender, KeyEventArgs e)
        {
            TreeNode t = treCharacterList.SelectedNode;
            CharacterCache objCache = t?.Tag as CharacterCache;
            objCache?.OnMyKeyDown(sender, new Tuple<KeyEventArgs, TreeNode>(e, t));
        }

        private static void treCharacterList_OnDefaultDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treCharacterList_OnDefaultDragOver(object sender, DragEventArgs e)
        {
            if (!(sender is TreeView treSenderView) || e == null)
                return;
            Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = treSenderView.GetNodeAt(pt);
            if (objNode != null)
            {
                if (objNode.Parent != null)
                    objNode = objNode.Parent;
                if (objNode.Tag?.ToString() != "Watch")
                {
                    objNode.BackColor = ColorManager.ControlDarker;
                }
            }

            // Clear the background color for all other Nodes.
            treCharacterList.ClearNodeBackground(objNode);
        }

        private static void treCharacterList_OnDefaultDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!(sender is TreeView) || e == null)
                return;
            //Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = e.Node;
            if (objNode?.Tag is Action act)
            {
                act.Invoke();
            }
        }

        private void treCharacterList_OnDefaultDragDrop(object sender, DragEventArgs e)
        {
            // Do not allow the root element to be moved.
            if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 || treCharacterList.SelectedNode.Parent?.Tag?.ToString() == "Watch")
                return;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                if (!(sender is TreeView treSenderView))
                    return;
                Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                if (nodDestinationNode?.Level > 0)
                    nodDestinationNode = nodDestinationNode.Parent;
                string strDestinationNode = nodDestinationNode?.Tag?.ToString();
                if (strDestinationNode != "Watch")
                {
                    if (!(e.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                        return;

                    if (nodNewNode.Level == 0 || nodNewNode.Parent == nodDestinationNode)
                        return;
                    if (nodNewNode.Tag is CharacterCache objCache)
                    {
                        switch (strDestinationNode)
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

            if (t?.Tag is CharacterCache objCache)
            {
                switch (t.Parent.Tag.ToString())
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
            if (objSelectedNode?.Tag == null || objSelectedNode.Level <= 0)
                return;
            string strFile = objSelectedNode.Tag.ToString();
            if (string.IsNullOrEmpty(strFile))
                return;
            Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
            if (objOpenCharacter != null)
            {
                using (new CursorWait(this))
                    Program.MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objOpenCharacter)?.Close();
            }
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
                if (!string.IsNullOrEmpty(strTag))
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
            int intToolStripWidth = 180;
            int intToolStripHeight = 22;
            using (Graphics g = CreateGraphics())
            {
                intToolStripWidth = (int)(intToolStripWidth * g.DpiX / 96.0f);
                intToolStripHeight = (int)(intToolStripHeight * g.DpiY / 96.0f);
            }

            //
            // tsToggleFav
            //
            DpiFriendlyToolStripMenuItem tsToggleFav = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.asterisk_orange,
                Name = "tsToggleFav",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_ToggleFavorite",
                ImageDpi192 = Properties.Resources.asterisk_orange1
            };
            tsToggleFav.Click += tsToggleFav_Click;
            //
            // tsSort
            //
            DpiFriendlyToolStripMenuItem tsSort = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.page_refresh,
                Name = "tsSort",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Sort",
                ImageDpi192 = Properties.Resources.page_refresh1
            };
            tsSort.Click += tsSort_Click;
            //
            // tsDelete
            //
            DpiFriendlyToolStripMenuItem tsDelete = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.delete,
                Name = "tsDelete",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Delete",
                ImageDpi192 = Properties.Resources.delete1
            };
            tsDelete.Click += tsDelete_Click;
            //
            // cmsRoster
            //
            ContextMenuStrip cmsRoster = new ContextMenuStrip
            {
                Name = "cmsRoster",
                Size = new Size(intToolStripWidth, intToolStripHeight * 5)
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
                DpiFriendlyToolStripMenuItem tsCloseOpenCharacter = new DpiFriendlyToolStripMenuItem
                {
                    Image = Properties.Resources.door_out,
                    Name = "tsCloseOpenCharacter",
                    Size = new Size(intToolStripWidth, intToolStripHeight),
                    Tag = "Menu_Close",
                    ImageDpi192 = Properties.Resources.door_out1
                };
                tsCloseOpenCharacter.Click += tsCloseOpenCharacter_Click;
                cmsRoster.Items.Add(tsCloseOpenCharacter);
                tsCloseOpenCharacter.TranslateToolStripItemsRecursively();
            }
            cmsRoster.UpdateLightDarkMode();

            return cmsRoster;
        }
    }
}
