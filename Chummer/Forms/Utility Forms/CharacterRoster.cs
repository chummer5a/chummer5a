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
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Chummer.Annotations;
using Chummer.Plugins;
using Newtonsoft.Json;
using NLog;

namespace Chummer
{
    public partial class CharacterRoster : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        private readonly FileSystemWatcher _watcherCharacterRosterFolder;
        private Task _tskMostRecentlyUsedsRefresh;
        private Task _tskWatchFolderRefresh;
        private CancellationTokenSource _objMostRecentlyUsedsRefreshCancellationTokenSource;
        private CancellationTokenSource _objWatchFolderRefreshCancellationTokenSource;

        public CharacterRoster()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            if (!string.IsNullOrEmpty(GlobalSettings.CharacterRosterPath) && Directory.Exists(GlobalSettings.CharacterRosterPath))
            {
                _watcherCharacterRosterFolder = new FileSystemWatcher(GlobalSettings.CharacterRosterPath, "*.chum5");
            }
        }

        public void SetMyEventHandlers(bool deleteThem = false)
        {
            if (!deleteThem)
            {
                Program.MainForm.OpenCharacterForms.CollectionChanged += OpenCharacterFormsOnCollectionChanged;
                GlobalSettings.MruChanged += RefreshMruLists;
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
                Program.MainForm.OpenCharacterForms.CollectionChanged -= OpenCharacterFormsOnCollectionChanged;
                GlobalSettings.MruChanged -= RefreshMruLists;
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

            void treCharacterList_OnDefaultDragEnter(object sender, DragEventArgs e)
            {
                e.Effect = DragDropEffects.Move;
            }

            void treCharacterList_OnDefaultDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
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
        }

        private async void CharacterRoster_Load(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                SetMyEventHandlers();
                _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
                _tskMostRecentlyUsedsRefresh
                    = LoadMruCharacters(true, _objMostRecentlyUsedsRefreshCancellationTokenSource.Token);
                _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
                _tskWatchFolderRefresh = LoadWatchFolderCharacters();
                try
                {
                    await Task.WhenAll(_tskMostRecentlyUsedsRefresh, _tskWatchFolderRefresh,
                                       Task.WhenAll(Program.PluginLoader.MyActivePlugins.Select(RefreshPluginNodes)));
                }
                catch (TaskCanceledException)
                {
                    //swallow this
                }

                await this.DoThreadSafeAsync(
                    () => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache));
            }
        }

        private bool _blnIsClosing;

        private void CharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_blnIsClosing)
                return;
            _blnIsClosing = true; // Needed to prevent crashes on disposal
            _objMostRecentlyUsedsRefreshCancellationTokenSource?.Cancel(false);
            _objWatchFolderRefreshCancellationTokenSource?.Cancel(false);

            SetMyEventHandlers(true);

            foreach (CharacterCache objCache in _dicSavedCharacterCaches.Values)
                objCache.Dispose();
            _dicSavedCharacterCaches.Dispose();

            DisposeTagNodes(treCharacterList.Nodes);

            void DisposeTagNodes(TreeNodeCollection lstNodes)
            {
                foreach (TreeNode nodNode in lstNodes)
                {
                    if (nodNode.Tag is CharacterCache objCache)
                        objCache.Dispose();
                    DisposeTagNodes(nodNode.Nodes);
                }
            }
        }

        private async void RefreshWatchList(object sender, EventArgs e)
        {
            if (_objWatchFolderRefreshCancellationTokenSource != null)
            {
                _objWatchFolderRefreshCancellationTokenSource.Cancel(false);
                _objWatchFolderRefreshCancellationTokenSource.Dispose();
            }
            _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskWatchFolderRefresh?.IsCompleted == false)
                    await _tskWatchFolderRefresh;
            }
            catch (TaskCanceledException)
            {
                //swallow this
            }

            if (this.IsNullOrDisposed())
                return;

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
            catch (TaskCanceledException)
            {
                //swallow this
            }
            ResumeLayout();
            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;
            await this.DoThreadSafeAsync(() => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache));
            PurgeUnusedCharacterCaches();
        }

        private async void RefreshMruLists(object sender, TextEventArgs e)
        {
            await RefreshMruLists(e?.Text);
        }

        public async ValueTask RefreshMruLists(string strMruType)
        {
            if (_objMostRecentlyUsedsRefreshCancellationTokenSource != null)
            {
                _objMostRecentlyUsedsRefreshCancellationTokenSource.Cancel(false);
                _objMostRecentlyUsedsRefreshCancellationTokenSource.Dispose();
            }
            _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskMostRecentlyUsedsRefresh?.IsCompleted == false)
                    await _tskMostRecentlyUsedsRefresh;
            }
            catch (TaskCanceledException)
            {
                //swallow this
            }

            if (this.IsNullOrDisposed())
                return;

            SuspendLayout();
            _tskMostRecentlyUsedsRefresh = LoadMruCharacters(strMruType != "mru", _objMostRecentlyUsedsRefreshCancellationTokenSource.Token);
            try
            {
                await _tskMostRecentlyUsedsRefresh;
            }
            catch (ObjectDisposedException)
            {
                //swallow this
            }
            catch (TaskCanceledException)
            {
                //swallow this
            }
            ResumeLayout();
            if (_objMostRecentlyUsedsRefreshCancellationTokenSource.IsCancellationRequested)
                return;
            await this.DoThreadSafeAsync(() => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache));
            PurgeUnusedCharacterCaches();
        }

        private async void OpenCharacterFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    await RefreshNodeTexts();
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Remove:
                {
                    // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                    if (e.OldItems.Cast<CharacterShared>()
                         .Any(objForm => !GlobalSettings.FavoriteCharacters.Contains(objForm.CharacterObject.FileName)
                                         && !GlobalSettings.MostRecentlyUsedCharacters.Contains(
                                             objForm.CharacterObject.FileName)))
                        await RefreshMruLists("mru");
                    else
                        await RefreshNodeTexts();
                }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    await RefreshMruLists(string.Empty);
                    break;
            }
        }

        public async ValueTask RefreshNodeTexts()
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

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
                    strTooltip += await LanguageManager.GetStringAsync("String_Error") + await LanguageManager.GetStringAsync("String_Colon") + Environment.NewLine + objCache.ErrorText;
                }
                else
                    treCharacterList.QueueThreadSafe(() => objCharacterNode.ForeColor = ColorManager.WindowText);
                treCharacterList.QueueThreadSafe(() => objCharacterNode.ToolTipText = strTooltip);
            }
        }

        private async Task LoadMruCharacters(bool blnRefreshFavorites, CancellationToken objCancellationToken)
        {
            if (objCancellationToken.IsCancellationRequested)
                return;

            if (treCharacterList.IsNullOrDisposed())
                return;

            List<string> lstFavorites = GlobalSettings.FavoriteCharacters.ToList();
            bool blnAddFavoriteNode = false;
            TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
            if (objFavoriteNode == null && blnRefreshFavorites)
            {
                objFavoriteNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_FavoriteCharacters"))
                    {Tag = "Favorite"};
                blnAddFavoriteNode = true;
            }

            if (objCancellationToken.IsCancellationRequested)
                return;

            bool blnAddRecentNode = false;
            List<string> lstRecents = new List<string>(GlobalSettings.MostRecentlyUsedCharacters);
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
            if (!blnRefreshFavorites)
                lstFavorites.Clear();
            TreeNode objRecentNode = treCharacterList.FindNode("Recent", false);
            if (objRecentNode == null && lstRecents.Count > 0)
            {
                objRecentNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_RecentCharacters"))
                    {Tag = "Recent"};
                blnAddRecentNode = true;
            }

            if (objCancellationToken.IsCancellationRequested)
                return;

            int intFavoritesCount = lstFavorites.Count;
            int intRecentsCount = lstRecents.Count;

            TreeNode[] lstFavoritesNodes = intFavoritesCount > 0 ? new TreeNode[intFavoritesCount] : null;
            TreeNode[] lstRecentsNodes = intRecentsCount > 0 ? new TreeNode[intRecentsCount] : null;

            if (intFavoritesCount > 0 || intRecentsCount > 0)
            {
                if (objCancellationToken.IsCancellationRequested)
                    return;
                Task<TreeNode>[] atskCachingTasks = new Task<TreeNode>[intFavoritesCount + intRecentsCount];

                for (int i = 0; i < intFavoritesCount; ++i)
                {
                    int iLocal = i;
                    atskCachingTasks[i]
                        = Task.Run(() => CacheCharacter(lstFavorites[iLocal]), objCancellationToken);
                }

                for (int i = 0; i < intRecentsCount; ++i)
                {
                    int iLocal = i;
                    atskCachingTasks[intFavoritesCount + i]
                        = Task.Run(() => CacheCharacter(lstRecents[iLocal]), objCancellationToken);
                }

                try
                {
                    await Task.WhenAll(atskCachingTasks);
                }
                catch (TaskCanceledException)
                {
                    //swallow this
                }

                if (lstFavoritesNodes != null)
                {
                    for (int i = 0; i < intFavoritesCount; ++i)
                    {
                        lstFavoritesNodes[i] = await atskCachingTasks[i];
                    }

                    if (objFavoriteNode != null)
                    {
                        foreach (TreeNode objNode in lstFavoritesNodes)
                        {
                            if (objCancellationToken.IsCancellationRequested)
                                return;
                            if (objNode == null)
                                continue;
                            if (objFavoriteNode.TreeView != null)
                            {
                                if (objFavoriteNode.TreeView.IsDisposed)
                                    continue;
                                await objFavoriteNode.TreeView.DoThreadSafeAsync(
                                    () => objFavoriteNode.Nodes.Add(objNode));
                            }
                            else
                                objFavoriteNode.Nodes.Add(objNode);
                        }
                    }
                }

                if (lstRecentsNodes != null)
                {
                    for (int i = 0; i < intRecentsCount; ++i)
                    {
                        lstRecentsNodes[i] = await atskCachingTasks[intFavoritesCount + i];
                    }

                    if (objRecentNode != null)
                    {
                        foreach (TreeNode objNode in lstRecentsNodes)
                        {
                            if (objCancellationToken.IsCancellationRequested)
                                return;
                            if (objNode == null)
                                continue;
                            if (objRecentNode.TreeView != null)
                            {
                                if (objRecentNode.TreeView.IsDisposed)
                                    continue;
                                await objRecentNode.TreeView.DoThreadSafeAsync(
                                    () => objRecentNode.Nodes.Add(objNode));
                            }
                            else
                                objRecentNode.Nodes.Add(objNode);
                        }
                    }
                }
            }

            if (objCancellationToken.IsCancellationRequested)
                return;

            if (treCharacterList.IsNullOrDisposed())
                return;

            Log.Trace("Populating CharacterRosterTreeNode MRUs (MainThread).");
            await treCharacterList.DoThreadSafeAsync(x =>
            {
                TreeView treList = (TreeView) x;
                treList.SuspendLayout();
                if (blnRefreshFavorites && objFavoriteNode != null)
                {
                    if (blnAddFavoriteNode)
                    {
                        treList.Nodes.Insert(0, objFavoriteNode);
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
                        treList.Nodes.Insert(objFavoriteNode != null ? 1 : 0, objRecentNode);
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
                treList.ResumeLayout();
            });
        }

        private async Task LoadWatchFolderCharacters()
        {
            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            if (treCharacterList.IsNullOrDisposed())
                return;

            Dictionary<string, string> dicWatch = null;
            if (!string.IsNullOrEmpty(GlobalSettings.CharacterRosterPath) && Directory.Exists(GlobalSettings.CharacterRosterPath))
            {
                string[] astrFiles
                    = Directory.GetFiles(GlobalSettings.CharacterRosterPath, "*.chum5", SearchOption.AllDirectories);
                dicWatch = new Dictionary<string, string>(astrFiles.Length);
                foreach (string strFile in astrFiles)
                {
                    if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                        return;

                    FileInfo objInfo = new FileInfo(strFile);
                    if (objInfo.Directory == null || objInfo.Directory.FullName == GlobalSettings.CharacterRosterPath)
                    {
                        dicWatch.Add(strFile, "Watch");
                        continue;
                    }

                    string strNewParent = objInfo.Directory.FullName.Replace(GlobalSettings.CharacterRosterPath + Path.DirectorySeparatorChar, string.Empty);
                    dicWatch.Add(strFile, strNewParent);
                }
            }
            bool blnAddWatchNode = dicWatch?.Count > 0;
            TreeNode objWatchNode = treCharacterList.FindNode("Watch", false);
            if (blnAddWatchNode)
            {
                if (objWatchNode != null)
                    objWatchNode.Nodes.Clear();
                else
                    objWatchNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_WatchFolder")) { Tag = "Watch" };
            }
            else
                objWatchNode?.Remove();

            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            if (objWatchNode == null || !blnAddWatchNode || dicWatch.Count == 0 || _objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            Dictionary<TreeNode, string> dicWatchNodes = new Dictionary<TreeNode, string>(dicWatch.Count);
            List<Task<TreeNode>> lstCachingTasks = new List<Task<TreeNode>>(dicWatch.Count);
            foreach (string strKey in dicWatch.Keys)
                lstCachingTasks.Add(Task.Run(() => CacheCharacter(strKey),
                                             _objMostRecentlyUsedsRefreshCancellationTokenSource.Token));
            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;
            await Task.WhenAll(lstCachingTasks);
            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;
            foreach (Task<TreeNode> tskCachingTask in lstCachingTasks)
            {
                TreeNode objNode = await tskCachingTask;
                if (objNode.Tag is CharacterCache objCache)
                    dicWatchNodes.Add(objNode, dicWatch[objCache.FilePath]);
                if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                    return;
            }

            foreach (string s in dicWatchNodes.Values.Distinct().OrderBy(x => x))
            {
                if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                    return;
                if (s == "Watch")
                    continue;
                if (objWatchNode.TreeView != null)
                {
                    if (objWatchNode.TreeView.IsDisposed)
                        continue;
                    await objWatchNode.TreeView.DoThreadSafeAsync(
                        () => objWatchNode.Nodes.Add(new TreeNode(s) {Tag = s}));
                }
                else
                    objWatchNode.Nodes.Add(new TreeNode(s) {Tag = s});
            }

            foreach (KeyValuePair<TreeNode, string> kvtNode in dicWatchNodes.OrderBy(x => x.Key.Text))
            {
                if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                    return;
                if (kvtNode.Value == "Watch")
                {
                    if (objWatchNode.TreeView != null)
                    {
                        if (objWatchNode.TreeView.IsDisposed)
                            continue;
                        await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key));
                    }
                    else
                        objWatchNode.Nodes.Add(kvtNode.Key);
                }
                else
                {
                    foreach (TreeNode objNode in objWatchNode.Nodes)
                    {
                        if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                            return;
                        if (objNode.Tag.ToString() != kvtNode.Value)
                            continue;
                        if (objWatchNode.TreeView != null)
                        {
                            if (objWatchNode.TreeView.IsDisposed)
                                continue;
                            await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key));
                        }
                        else
                            objNode.Nodes.Add(kvtNode.Key);
                    }
                }
            }

            if (_objWatchFolderRefreshCancellationTokenSource.IsCancellationRequested)
                return;

            if (treCharacterList.IsNullOrDisposed())
                return;

            Log.Trace("Populating CharacterRosterTreeNode Watch Folder (MainThread).");
            await treCharacterList.DoThreadSafeAsync(x =>
            {
                TreeView treList = (TreeView)x;
                treList.SuspendLayout();
                if (objWatchNode != null)
                {
                    if (objWatchNode.TreeView == null)
                    {
                        TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
                        TreeNode objRecentNode = treCharacterList.FindNode("Recent", false);
                        if (objFavoriteNode != null && objRecentNode != null)
                            treList.Nodes.Insert(2, objWatchNode);
                        else if (objFavoriteNode != null || objRecentNode != null)
                            treList.Nodes.Insert(1, objWatchNode);
                        else
                            treList.Nodes.Insert(0, objWatchNode);
                    }
                    objWatchNode.ExpandAll();
                }
                treList.ResumeLayout();
            });
        }

        public Task RefreshPluginNodes(IPlugin objPluginToRefresh)
        {
            if (objPluginToRefresh == null)
                throw new ArgumentNullException(nameof(objPluginToRefresh));
            return RefreshPluginNodesInner(objPluginToRefresh); // Split up this way so that the parameter check happens synchronously
        }

        private async Task RefreshPluginNodesInner([NotNull] IPlugin objPluginToRefresh)
        {
            int intNodeOffset = Program.PluginLoader.MyActivePlugins.IndexOf(objPluginToRefresh);
            if (intNodeOffset >= 0)
            {
                await Task.Run(async () =>
                {
                    Log.Info("Starting new Task to get CharacterRosterTreeNodes for plugin:" + objPluginToRefresh);
                    List<TreeNode> lstNodes =
                        (await objPluginToRefresh.GetCharacterRosterTreeNode(this, true))?.ToList();
                    if (lstNodes != null)
                    {
                        lstNodes.Sort((x, y) => string.CompareOrdinal(x.Text, y.Text));
                        for (int i = 0; i < lstNodes.Count; ++i)
                        {
                            TreeNode node = lstNodes[i];
                            string strNodeText = node.Text;
                            object objNodeTag = node.Tag;
                            TreeNode objExistingNode = await treCharacterList.DoThreadSafeFuncAsync(x =>
                                ((TreeView)x).Nodes.Cast<TreeNode>()
                                                .FirstOrDefault(y => y.Text == strNodeText && y.Tag == objNodeTag));
                            try
                            {
                                await treCharacterList.DoThreadSafeAsync(x =>
                                {
                                    TreeView treList = (TreeView)x;
                                    if (objExistingNode != null)
                                    {
                                        treList.Nodes.Remove(objExistingNode);
                                    }

                                    if (node.Nodes.Count > 0 || !string.IsNullOrEmpty(node.ToolTipText)
                                                             || objNodeTag != null)
                                    {
                                        if (treList.Nodes.ContainsKey(node.Name))
                                            treList.Nodes.RemoveByKey(node.Name);
                                        TreeNode objFavoriteNode = treList.FindNode("Favorite", false);
                                        TreeNode objRecentNode = treList.FindNode("Recent", false);
                                        TreeNode objWatchNode = treList.FindNode("Watch", false);
                                        if (objFavoriteNode != null && objRecentNode != null && objWatchNode != null)
                                            treList.Nodes.Insert(i + intNodeOffset + 3, node);
                                        else if (objFavoriteNode != null || objRecentNode != null
                                                                         || objWatchNode != null)
                                        {
                                            if ((objFavoriteNode != null && objRecentNode != null) ||
                                                (objFavoriteNode != null && objWatchNode != null) ||
                                                (objRecentNode != null && objWatchNode != null))
                                                treList.Nodes.Insert(i + intNodeOffset + 2, node);
                                            else
                                                treList.Nodes.Insert(i + intNodeOffset + 1, node);
                                        }
                                        else
                                            treList.Nodes.Insert(i + intNodeOffset, node);
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
                    }

                    Log.Info("Task to get and add CharacterRosterTreeNodes for plugin " + objPluginToRefresh
                             + " finished.");
                });
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        private readonly LockingDictionary<string, CharacterCache> _dicSavedCharacterCaches = new LockingDictionary<string, CharacterCache>();

        /// <summary>
        /// Remove all character caches from the cached dictionary that are not present in any of the form's lists (and are therefore unnecessary).
        /// </summary>
        private void PurgeUnusedCharacterCaches()
        {
            foreach (CharacterCache objCache in _dicSavedCharacterCaches.Values.ToList())
            {
                if (treCharacterList.FindNodeByTag(objCache) == null)
                {
                    _dicSavedCharacterCaches.Remove(objCache.FilePath);
                    objCache.Dispose();
                }
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// The cache is then saved in a dictionary to prevent us from storing duplicate image data in memory (which can get expensive!)
        /// </summary>
        /// <param name="strFile"></param>
        private async Task<TreeNode> CacheCharacter(string strFile)
        {
            CharacterCache objCache = null;
            if (!_dicSavedCharacterCaches.IsDisposed)
            {
                try
                {
                    while (!_dicSavedCharacterCaches.TryGetValue(strFile, out objCache))
                    {
                        objCache = await CharacterCache.CreateFromFileAsync(strFile);
                        if (await _dicSavedCharacterCaches.TryAddAsync(strFile, objCache))
                            break;
                        objCache.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // We shouldn't be caching characters if we've already disposed ourselves, so if you break here,
                    // something went wrong (but not fatally so, which is why the exception is handled)
                    Utils.BreakIfDebug();
                    if (objCache == null)
                        objCache = await CharacterCache.CreateFromFileAsync(strFile);
                }
            }
            else
                objCache = await CharacterCache.CreateFromFileAsync(strFile);

            TreeNode objNode = new TreeNode
            {
                Text = objCache.CalculatedName(),
                ToolTipText = await objCache.FilePath.CheapReplaceAsync(Utils.GetStartupPath,
                                                                        () => '<' + Application.ProductName + '>'),
                Tag = objCache
            };
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                objNode.ForeColor = ColorManager.ErrorColor;
                if (!string.IsNullOrEmpty(objNode.ToolTipText))
                    objNode.ToolTipText += Environment.NewLine + Environment.NewLine;
                objNode.ToolTipText += await LanguageManager.GetStringAsync("String_Error") +
                                       await LanguageManager.GetStringAsync("String_Colon") + Environment.NewLine +
                                       objCache.ErrorText;
            }

            return objNode;
        }

        /// <summary>
        /// Update the labels and images based on the selected treenode.
        /// </summary>
        /// <param name="objCache"></param>
        public async Task UpdateCharacter(CharacterCache objCache)
        {
            if (this.IsNullOrDisposed()) // Safety check for external calls
                return;
            using (new CursorWait(this))
            {
                tlpRight.SuspendLayout();
                if (objCache != null)
                {
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                    txtCharacterBio.Text = objCache.Description.RtfToPlainText();
                    txtCharacterBackground.Text = objCache.Background.RtfToPlainText();
                    txtCharacterNotes.Text = objCache.CharacterNotes.RtfToPlainText();
                    txtGameNotes.Text = objCache.GameNotes.RtfToPlainText();
                    txtCharacterConcept.Text = objCache.Concept.RtfToPlainText();
                    lblCareerKarma.Text = objCache.Karma;
                    if (string.IsNullOrEmpty(lblCareerKarma.Text)
                        || lblCareerKarma.Text == 0.ToString(GlobalSettings.CultureInfo))
                        lblCareerKarma.Text = await LanguageManager.GetStringAsync("String_None");
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
                        lblFilePath.Text = await LanguageManager.GetStringAsync("MessageTitle_FileNotFound");
                    lblSettings.Text = objCache.SettingsFile;
                    if (string.IsNullOrEmpty(lblSettings.Text))
                        lblSettings.Text = strUnknown;
                    lblFilePath.SetToolTip(
                        await objCache.FilePath.CheapReplaceAsync(Utils.GetStartupPath,
                                                                  () => '<' + Application.ProductName + '>'));
                    picMugshot.Image = objCache.Mugshot;

                    // Populate character information fields.
                    if (objCache.Metatype != null)
                    {
                        XPathNavigator objMetatypeDoc = await XmlManager.LoadXPathAsync("metatypes.xml");
                        XPathNavigator objMetatypeNode
                            = objMetatypeDoc.SelectSingleNode(
                                "/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + ']');
                        if (objMetatypeNode == null)
                        {
                            objMetatypeDoc = await XmlManager.LoadXPathAsync("critters.xml");
                            objMetatypeNode = objMetatypeDoc.SelectSingleNode(
                                "/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + ']');
                        }

                        string strMetatype = objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate")?.Value
                                             ?? objCache.Metatype;

                        if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                        {
                            objMetatypeNode = objMetatypeNode?.SelectSingleNode(
                                "metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + ']');

                            strMetatype += await LanguageManager.GetStringAsync("String_Space") + '('
                                + (objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate")?.Value
                                   ?? objCache.Metavariant) + ')';
                        }

                        lblMetatype.Text = strMetatype;
                    }
                    else
                        lblMetatype.Text = await LanguageManager.GetStringAsync("String_MetatypeLoadError");

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
        }

        #region Form Methods

        private async void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode == null)
                return;
            CharacterCache objCache = objSelectedNode.Tag as CharacterCache;
            objCache?.OnMyAfterSelect?.Invoke(sender, e);
            await UpdateCharacter(objCache);
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
                                GlobalSettings.FavoriteCharacters.Remove(objCache.FilePath);
                                GlobalSettings.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                                break;

                            case "Favorite":
                                GlobalSettings.FavoriteCharacters.AddWithSort(objCache.FilePath);
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
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                objCache.OnMyContextMenuDeleteClick(t, e);
            }
        }

        private void tsSort_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalSettings.MostRecentlyUsedCharacters.Sort();
                        break;

                    case "Favorite":
                        GlobalSettings.FavoriteCharacters.Sort();
                        break;
                }
            }
            else if (t?.Tag != null)
            {
                switch (t.Tag.ToString())
                {
                    case "Recent":
                        GlobalSettings.MostRecentlyUsedCharacters.Sort();
                        break;

                    case "Favorite":
                        GlobalSettings.FavoriteCharacters.Sort();
                        break;
                }
            }
            treCharacterList.SelectedNode = t;
        }

        private void tsToggleFav_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Favorite":
                        GlobalSettings.FavoriteCharacters.Remove(objCache.FilePath);
                        GlobalSettings.MostRecentlyUsedCharacters.Insert(0, objCache.FilePath);
                        break;

                    default:
                        GlobalSettings.FavoriteCharacters.AddWithSort(objCache.FilePath);
                        break;
                }
                treCharacterList.SelectedNode = t;
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<MouseEventArgs> OnMyMouseDown { get; set; }

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
            if (treCharacterList.IsNullOrDisposed())
                return;
            TreeNode objSelectedNode = treCharacterList.SelectedNode;
            if (objSelectedNode?.Tag == null || objSelectedNode.Level <= 0)
                return;
            string strFile = objSelectedNode.Tag.ToString();
            if (string.IsNullOrEmpty(strFile))
                return;
            Character objOpenCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
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
            foreach (IPlugin plugin in Program.PluginLoader.MyActivePlugins)
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
