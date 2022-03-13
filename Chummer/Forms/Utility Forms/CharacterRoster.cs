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
            using (CursorWait.New(this))
            {
                SetMyEventHandlers();
                _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
                _tskMostRecentlyUsedsRefresh
                    = LoadMruCharacters(true, _objMostRecentlyUsedsRefreshCancellationTokenSource.Token);
                _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
                _tskWatchFolderRefresh = LoadWatchFolderCharacters(_objMostRecentlyUsedsRefreshCancellationTokenSource.Token);
                try
                {
                    await Task.WhenAll(_tskMostRecentlyUsedsRefresh, _tskWatchFolderRefresh,
                                       Task.WhenAll(Program.PluginLoader.MyActivePlugins.Select(RefreshPluginNodes)));
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }

                await this.DoThreadSafeAsync(
                    () => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache)
                        .ContinueWith(x => IsFinishedLoading = true));
            }
        }

        private bool _blnIsClosing;

        private async void CharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (CursorWait.New(this))
            {
                if (_blnIsClosing)
                    return;
                _blnIsClosing = true; // Needed to prevent crashes on disposal
                if (_objMostRecentlyUsedsRefreshCancellationTokenSource?.IsCancellationRequested == false)
                {
                    _objMostRecentlyUsedsRefreshCancellationTokenSource.Cancel(false);
                    _objMostRecentlyUsedsRefreshCancellationTokenSource.Dispose();
                    _objMostRecentlyUsedsRefreshCancellationTokenSource = null;
                }
                if (_objWatchFolderRefreshCancellationTokenSource?.IsCancellationRequested == false)
                {
                    _objWatchFolderRefreshCancellationTokenSource.Cancel(false);
                    _objWatchFolderRefreshCancellationTokenSource.Dispose();
                    _objWatchFolderRefreshCancellationTokenSource = null;
                }

                SetMyEventHandlers(true);

                await DisposeTagNodes(treCharacterList.Nodes);

                async ValueTask DisposeTagNodes(TreeNodeCollection lstNodes)
                {
                    foreach (TreeNode nodNode in lstNodes)
                    {
                        if (nodNode.Tag is CharacterCache objCache)
                        {
                            nodNode.Tag = null;
                            if (!objCache.IsDisposed)
                            {
                                await _dicSavedCharacterCaches.RemoveAsync(objCache.FilePath);
                                await objCache.DisposeAsync();
                            }
                        }

                        await DisposeTagNodes(nodNode.Nodes);
                    }
                }

                await _dicSavedCharacterCaches.ForEachAsync(async kvpCache =>
                                                                await kvpCache.Value.DisposeAsync()
                                                                              .ConfigureAwait(false))
                                              ;
                await _dicSavedCharacterCaches.DisposeAsync();

                try
                {
                    await _tskMostRecentlyUsedsRefresh;
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
                try
                {
                    await _tskWatchFolderRefresh;
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private async void RefreshWatchList(object sender, EventArgs e)
        {
            if (_objWatchFolderRefreshCancellationTokenSource?.IsCancellationRequested == false)
            {
                _objWatchFolderRefreshCancellationTokenSource.Cancel(false);
                _objWatchFolderRefreshCancellationTokenSource.Dispose();
                _objWatchFolderRefreshCancellationTokenSource = null;
            }
            _objWatchFolderRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskWatchFolderRefresh?.IsCompleted == false)
                    await _tskWatchFolderRefresh;
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }

            if (this.IsNullOrDisposed())
                return;

            SuspendLayout();
            try
            {
                _tskWatchFolderRefresh = LoadWatchFolderCharacters(_objWatchFolderRefreshCancellationTokenSource.Token);
                try
                {
                    await _tskWatchFolderRefresh;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
            finally
            {
                ResumeLayout();
            }

            await this.DoThreadSafeAsync(
                () => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache,
                                      _objWatchFolderRefreshCancellationTokenSource.Token),
                _objWatchFolderRefreshCancellationTokenSource.Token);
            await PurgeUnusedCharacterCaches(_objWatchFolderRefreshCancellationTokenSource.Token);
        }

        private async void RefreshMruLists(object sender, TextEventArgs e)
        {
            await RefreshMruLists(e?.Text);
        }

        public async ValueTask RefreshMruLists(string strMruType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_objMostRecentlyUsedsRefreshCancellationTokenSource?.IsCancellationRequested == false)
            {
                _objMostRecentlyUsedsRefreshCancellationTokenSource.Cancel(false);
                _objMostRecentlyUsedsRefreshCancellationTokenSource.Dispose();
                _objMostRecentlyUsedsRefreshCancellationTokenSource = null;
            }
            token.ThrowIfCancellationRequested();
            _objMostRecentlyUsedsRefreshCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskMostRecentlyUsedsRefresh?.IsCompleted == false)
                    await _tskMostRecentlyUsedsRefresh;
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }

            if (this.IsNullOrDisposed())
                return;

            CancellationToken innerToken = _objMostRecentlyUsedsRefreshCancellationTokenSource.Token;
            SuspendLayout();
            try
            {
                _tskMostRecentlyUsedsRefresh
                    = LoadMruCharacters(strMruType != "mru", innerToken);
                try
                {
                    await _tskMostRecentlyUsedsRefresh;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
            finally
            {
                ResumeLayout();
            }
            innerToken.ThrowIfCancellationRequested();
            token.ThrowIfCancellationRequested();
            await this.DoThreadSafeAsync(() => UpdateCharacter(treCharacterList.SelectedNode?.Tag as CharacterCache, token), token);
            await PurgeUnusedCharacterCaches(token);
        }

        private async void OpenCharacterFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        await RefreshNodeTexts();
                        break;

                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Remove:
                    {
                        bool blnRefreshMru = false;
                        // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                        foreach (CharacterShared objForm in e.OldItems)
                        {
                            if (await GlobalSettings.FavoriteCharacters.ContainsAsync(objForm.CharacterObject.FileName))
                                continue;
                            if (await GlobalSettings.MostRecentlyUsedCharacters.ContainsAsync(
                                    objForm.CharacterObject.FileName))
                                continue;
                            blnRefreshMru = true;
                            break;
                        }

                        if (blnRefreshMru)
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
            catch (OperationCanceledException)
            {
                // Swallow this
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

        private async Task LoadMruCharacters(bool blnRefreshFavorites, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (treCharacterList.IsNullOrDisposed())
                return;

            List<string> lstFavorites = (await GlobalSettings.FavoriteCharacters.ToArrayAsync()).ToList();
            bool blnAddFavoriteNode = false;
            TreeNode objFavoriteNode = treCharacterList.FindNode("Favorite", false);
            if (objFavoriteNode == null && blnRefreshFavorites)
            {
                objFavoriteNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_FavoriteCharacters"))
                    {Tag = "Favorite"};
                blnAddFavoriteNode = true;
            }

            token.ThrowIfCancellationRequested();

            bool blnAddRecentNode = false;
            List<string> lstRecents = (await GlobalSettings.MostRecentlyUsedCharacters.ToArrayAsync()).ToList();
            // Add any characters that are open to the displayed list so we can have more than 10 characters listed
            await Program.MainForm.OpenCharacterForms.ForEachAsync(objCharacterForm =>
            {
                string strFile = objCharacterForm.CharacterObject.FileName;
                // Make sure we're not loading a character that was already loaded by the MRU list.
                if (!lstFavorites.Contains(strFile) && !lstRecents.Contains(strFile))
                    lstRecents.Add(strFile);
            });
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

            token.ThrowIfCancellationRequested();

            int intFavoritesCount = lstFavorites.Count;
            int intRecentsCount = lstRecents.Count;

            TreeNode[] lstFavoritesNodes = intFavoritesCount > 0 ? new TreeNode[intFavoritesCount] : null;
            TreeNode[] lstRecentsNodes = intRecentsCount > 0 ? new TreeNode[intRecentsCount] : null;

            if (intFavoritesCount > 0 || intRecentsCount > 0)
            {
                token.ThrowIfCancellationRequested();
                Task<TreeNode>[] atskCachingTasks = new Task<TreeNode>[intFavoritesCount + intRecentsCount];

                for (int i = 0; i < intFavoritesCount; ++i)
                {
                    int iLocal = i;
                    atskCachingTasks[i]
                        = Task.Run(() => CacheCharacter(lstFavorites[iLocal], token), token);
                }

                for (int i = 0; i < intRecentsCount; ++i)
                {
                    int iLocal = i;
                    atskCachingTasks[intFavoritesCount + i]
                        = Task.Run(() => CacheCharacter(lstRecents[iLocal], token), token);
                }

                try
                {
                    await Task.WhenAll(atskCachingTasks);
                }
                catch (OperationCanceledException)
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
                            token.ThrowIfCancellationRequested();
                            if (objNode == null)
                                continue;
                            if (objFavoriteNode.TreeView != null)
                            {
                                if (objFavoriteNode.TreeView.IsDisposed)
                                    continue;
                                await objFavoriteNode.TreeView.DoThreadSafeAsync(
                                    () => objFavoriteNode.Nodes.Add(objNode), token);
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
                            token.ThrowIfCancellationRequested();
                            if (objNode == null)
                                continue;
                            if (objRecentNode.TreeView != null)
                            {
                                if (objRecentNode.TreeView.IsDisposed)
                                    continue;
                                await objRecentNode.TreeView.DoThreadSafeAsync(
                                    () => objRecentNode.Nodes.Add(objNode), token);
                            }
                            else
                                objRecentNode.Nodes.Add(objNode);
                        }
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            if (treCharacterList.IsNullOrDisposed())
                return;

            Log.Trace("Populating CharacterRosterTreeNode MRUs (MainThread).");
            await treCharacterList.DoThreadSafeAsync(treList =>
            {
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
            }, token);
        }

        private async Task LoadWatchFolderCharacters(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

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
                    token.ThrowIfCancellationRequested();

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

            token.ThrowIfCancellationRequested();

            if (objWatchNode == null || !blnAddWatchNode || dicWatch.Count == 0)
                return;

            Dictionary<TreeNode, string> dicWatchNodes = new Dictionary<TreeNode, string>(dicWatch.Count);
            List<Task<TreeNode>> lstCachingTasks = new List<Task<TreeNode>>(dicWatch.Count);
            foreach (string strKey in dicWatch.Keys)
                lstCachingTasks.Add(Task.Run(() => CacheCharacter(strKey, token),
                                             _objMostRecentlyUsedsRefreshCancellationTokenSource.Token));
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstCachingTasks);
            token.ThrowIfCancellationRequested();
            foreach (Task<TreeNode> tskCachingTask in lstCachingTasks)
            {
                TreeNode objNode = await tskCachingTask;
                if (objNode.Tag is CharacterCache objCache)
                    dicWatchNodes.Add(objNode, dicWatch[objCache.FilePath]);
                token.ThrowIfCancellationRequested();
            }

            foreach (string s in dicWatchNodes.Values.Distinct().OrderBy(x => x))
            {
                token.ThrowIfCancellationRequested();
                if (s == "Watch")
                    continue;
                if (objWatchNode.TreeView != null)
                {
                    if (objWatchNode.TreeView.IsDisposed)
                        continue;
                    await objWatchNode.TreeView.DoThreadSafeAsync(
                        () => objWatchNode.Nodes.Add(new TreeNode(s) {Tag = s}), token);
                }
                else
                    objWatchNode.Nodes.Add(new TreeNode(s) {Tag = s});
            }

            foreach (KeyValuePair<TreeNode, string> kvtNode in dicWatchNodes.OrderBy(x => x.Key.Text))
            {
                token.ThrowIfCancellationRequested();
                if (kvtNode.Value == "Watch")
                {
                    if (objWatchNode.TreeView != null)
                    {
                        if (objWatchNode.TreeView.IsDisposed)
                            continue;
                        await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key), token);
                    }
                    else
                        objWatchNode.Nodes.Add(kvtNode.Key);
                }
                else
                {
                    foreach (TreeNode objNode in objWatchNode.Nodes)
                    {
                        token.ThrowIfCancellationRequested();
                        if (objNode.Tag.ToString() != kvtNode.Value)
                            continue;
                        if (objWatchNode.TreeView != null)
                        {
                            if (objWatchNode.TreeView.IsDisposed)
                                continue;
                            await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key), token);
                        }
                        else
                            objNode.Nodes.Add(kvtNode.Key);
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            if (treCharacterList.IsNullOrDisposed())
                return;

            Log.Trace("Populating CharacterRosterTreeNode Watch Folder (MainThread).");
            await treCharacterList.DoThreadSafeAsync(x =>
            {
                TreeView treList = x;
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
            }, token);
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
                                x.Nodes.Cast<TreeNode>()
                                 .FirstOrDefault(y => y.Text == strNodeText && y.Tag == objNodeTag));
                            try
                            {
                                await treCharacterList.DoThreadSafeAsync(x =>
                                {
                                    TreeView treList = x;
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
        private async ValueTask PurgeUnusedCharacterCaches(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (KeyValuePair<string, CharacterCache> kvpCache in await _dicSavedCharacterCaches.ToArrayAsync())
            {
                token.ThrowIfCancellationRequested();
                CharacterCache objCache = kvpCache.Value;
                if (treCharacterList.FindNodeByTag(objCache) != null)
                    continue;
                token.ThrowIfCancellationRequested();
                await _dicSavedCharacterCaches.RemoveAsync(objCache.FilePath);
                if (!objCache.IsDisposed)
                    await objCache.DisposeAsync();
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// The cache is then saved in a dictionary to prevent us from storing duplicate image data in memory (which can get expensive!)
        /// </summary>
        private async Task<TreeNode> CacheCharacter(string strFile, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CharacterCache objCache = null;
            if (!_dicSavedCharacterCaches.IsDisposed)
            {
                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        bool blnSuccess;
                        (blnSuccess, objCache) = await _dicSavedCharacterCaches.TryGetValueAsync(strFile);
                        if (blnSuccess)
                            break;
                        objCache = await CharacterCache.CreateFromFileAsync(strFile);
                        if (await _dicSavedCharacterCaches.TryAddAsync(strFile, objCache))
                            break;
                        await objCache.DisposeAsync();
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
            token.ThrowIfCancellationRequested();
            if (objCache == null)
                return new TreeNode
                    {Text = await LanguageManager.GetStringAsync("String_Error"), ForeColor = ColorManager.ErrorColor};
            token.ThrowIfCancellationRequested();
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
        public async Task UpdateCharacter(CharacterCache objCache, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (await this.DoThreadSafeFuncAsync(x => x.IsNullOrDisposed(), token)) // Safety check for external calls
                return;
            using (CursorWait.New(this))
            {
                await tlpRight.DoThreadSafeAsync(x => x.SuspendLayout(), token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (objCache != null)
                    {
                        string strUnknown = await LanguageManager.GetStringAsync("String_Unknown");
                        string strText = await objCache.Description.RtfToPlainTextAsync();
                        await txtCharacterBio.DoThreadSafeAsync(x => x.Text = strText, token);
                        strText = await objCache.Background.RtfToPlainTextAsync();
                        await txtCharacterBackground.DoThreadSafeAsync(x => x.Text = strText, token);
                        strText = await objCache.CharacterNotes.RtfToPlainTextAsync();
                        await txtCharacterNotes.DoThreadSafeAsync(x => x.Text = strText, token);
                        strText = await objCache.GameNotes.RtfToPlainTextAsync();
                        await txtGameNotes.DoThreadSafeAsync(x => x.Text = strText, token);
                        strText = await objCache.Concept.RtfToPlainTextAsync();
                        await txtCharacterConcept.DoThreadSafeAsync(x => x.Text = strText, token);
                        strText = objCache.Karma;
                        if (string.IsNullOrEmpty(strText) || strText == 0.ToString(GlobalSettings.CultureInfo))
                            strText = await LanguageManager.GetStringAsync("String_None");
                        await lblCareerKarma.DoThreadSafeAsync(x => x.Text = strText, token);
                        await lblPlayerName.DoThreadSafeAsync(x =>
                        {
                            x.Text = objCache.PlayerName;
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strUnknown;
                        }, token);
                        await lblCharacterName.DoThreadSafeAsync(x =>
                        {
                            x.Text = objCache.CharacterName;
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strUnknown;
                        }, token);
                        await lblCharacterAlias.DoThreadSafeAsync(x =>
                        {
                            x.Text = objCache.CharacterAlias;
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strUnknown;
                        }, token);
                        await lblEssence.DoThreadSafeAsync(x =>
                        {
                            x.Text = objCache.Essence;
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strUnknown;
                        }, token);
                        strText = objCache.FileName;
                        if (string.IsNullOrEmpty(strText))
                            strText = await LanguageManager.GetStringAsync("MessageTitle_FileNotFound");
                        await lblFilePath.DoThreadSafeAsync(x =>
                        {
                            x.Text = strText;
                        }, token);
                        await lblSettings.DoThreadSafeAsync(x =>
                        {
                            x.Text = objCache.SettingsFile;
                            if (string.IsNullOrEmpty(x.Text))
                                x.Text = strUnknown;
                        }, token);
                        await lblFilePath.SetToolTipAsync(
                            await objCache.FilePath.CheapReplaceAsync(Utils.GetStartupPath,
                                                                      () => '<' + Application.ProductName + '>'), token);
                        await picMugshot.DoThreadSafeAsync(x => x.Image = objCache.Mugshot, token);
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
                            token.ThrowIfCancellationRequested();
                            string strMetatype = objMetatypeNode != null
                                ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value
                                  ?? objCache.Metatype
                                : objCache.Metatype;

                            if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                            {
                                objMetatypeNode = objMetatypeNode?.SelectSingleNode(
                                    "metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + ']');

                                strMetatype += await LanguageManager.GetStringAsync("String_Space") + '('
                                    + (objMetatypeNode != null
                                        ? (await objMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate"))
                                          ?.Value
                                          ?? objCache.Metavariant
                                        : objCache.Metavariant) + ')';
                            }

                            await lblMetatype.DoThreadSafeAsync(x => x.Text = strMetatype, token);
                        }
                        else
                        {
                            strText = await LanguageManager.GetStringAsync("String_MetatypeLoadError");
                            await lblMetatype.DoThreadSafeAsync(x => x.Text = strText, token);
                        }

                        await tabCharacterText.DoThreadSafeAsync(x => x.Visible = true, token);
                        if (!string.IsNullOrEmpty(objCache.ErrorText))
                        {
                            await txtCharacterBio.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.ErrorText;
                                x.ForeColor = ColorManager.ErrorColor;
                                x.BringToFront();
                            }, token);
                        }
                        else
                            await txtCharacterBio.DoThreadSafeAsync(x => x.ForeColor = ColorManager.WindowText, token);
                    }
                    else
                    {
                        await tabCharacterText.DoThreadSafeAsync(x => x.Visible = false, token);
                        await txtCharacterBio.DoThreadSafeAsync(x => x.Clear(), token);
                        await txtCharacterBackground.DoThreadSafeAsync(x => x.Clear(), token);
                        await txtCharacterNotes.DoThreadSafeAsync(x => x.Clear(), token);
                        await txtGameNotes.DoThreadSafeAsync(x => x.Clear(), token);
                        await txtCharacterConcept.DoThreadSafeAsync(x => x.Clear(), token);
                        await lblCareerKarma.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblMetatype.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblPlayerName.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblCharacterName.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblCharacterAlias.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblEssence.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblFilePath.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await lblFilePath.SetToolTipAsync(string.Empty, token);
                        await lblSettings.DoThreadSafeAsync(x => x.Text = string.Empty, token);
                        await picMugshot.DoThreadSafeAsync(x => x.Image = null, token);
                    }
                    await lblCareerKarmaLabel.DoThreadSafeAsync(
                        x => x.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text), token);
                    await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblMetatype.Text), token);
                    await lblPlayerNameLabel.DoThreadSafeAsync(
                        x => x.Visible = !string.IsNullOrEmpty(lblPlayerName.Text), token);
                    await lblCharacterNameLabel.DoThreadSafeAsync(
                        x => x.Visible = !string.IsNullOrEmpty(lblCharacterName.Text), token);
                    await lblCharacterAliasLabel.DoThreadSafeAsync(
                        x => x.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text), token);
                    await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblEssence.Text), token);
                    await lblFilePathLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblFilePath.Text), token);
                    await lblSettingsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblSettings.Text), token);
                    await ProcessMugshotSizeMode(token);
                }
                finally
                {
                    await tlpRight.DoThreadSafeAsync(x => x.ResumeLayout(), token);
                }
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
                        using (CursorWait.New(this))
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

        private async void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            await ProcessMugshotSizeMode();
        }

        private Task ProcessMugshotSizeMode(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return Task.CompletedTask;
            return picMugshot.DoThreadSafeAsync(x =>
            {
                if (x.IsNullOrDisposed())
                    return;
                try
                {
                    x.SizeMode = x.Image != null && x.Height >= x.Image.Height && x.Width >= x.Image.Width
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

        private async void tsOpen_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                using (CursorWait.New(this))
                {
                    Character objOpenCharacter
                        = Program.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FileName)
                          ?? await Program.LoadCharacterAsync(objCache.FilePath);
                    if (!Program.SwitchToOpenCharacter(objOpenCharacter))
                        await Program.OpenCharacter(objOpenCharacter);
                }
            }
        }

        private async void tsOpenForPrinting_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                using (CursorWait.New(this))
                {
                    Character objCharacter
                        = Program.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FileName)
                          ?? await Program.LoadCharacterAsync(objCache.FilePath);
                    try
                    {
                        await Program.OpenCharacterForPrinting(objCharacter);
                    }
                    finally
                    {
                        if (await Program.OpenCharacters.AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                            && await Program.MainForm.OpenCharacterForms.AllAsync(x => x.CharacterObject != objCharacter))
                            Program.OpenCharacters.Remove(objCharacter);
                        await objCharacter.DisposeAsync();
                    }
                }
            }
        }

        private async void tsOpenForExport_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                using (CursorWait.New(this))
                {
                    Character objCharacter
                        = Program.OpenCharacters.FirstOrDefault(x => x.FileName == objCache.FileName)
                          ?? await Program.LoadCharacterAsync(objCache.FilePath);
                    try
                    {
                        await Program.OpenCharacterForExport(objCharacter);
                    }
                    finally
                    {
                        if (await Program.OpenCharacters.AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                            && await Program.MainForm.OpenCharacterForms.AllAsync(x => x.CharacterObject != objCharacter))
                            Program.OpenCharacters.Remove(objCharacter);
                        await objCharacter.DisposeAsync();
                    }
                }
            }
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
                using (CursorWait.New(this))
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
            // tsOpen
            //
            DpiFriendlyToolStripMenuItem tsOpen = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.folder_page,
                Name = "tsOpen",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_Open",
                ImageDpi192 = Properties.Resources.folder_page1
            };
            tsOpen.Click += tsOpen_Click;
            //
            // tsOpenForPrinting
            //
            DpiFriendlyToolStripMenuItem tsOpenForPrinting = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.folder_print,
                Name = "tsOpenForPrinting",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_OpenForPrinting",
                ImageDpi192 = Properties.Resources.folder_print1
            };
            tsOpenForPrinting.Click += tsOpenForPrinting_Click;
            //
            // tsOpenForExport
            //
            DpiFriendlyToolStripMenuItem tsOpenForExport = new DpiFriendlyToolStripMenuItem
            {
                Image = Properties.Resources.folder_script_go,
                Name = "tsOpenForExport",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_OpenForExport",
                ImageDpi192 = Properties.Resources.folder_script_go1
            };
            tsOpenForExport.Click += tsOpenForExport_Click;
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
                tsOpen,
                tsOpenForPrinting,
                tsOpenForExport,
                tsDelete
            });

            tsToggleFav.TranslateToolStripItemsRecursively();
            tsSort.TranslateToolStripItemsRecursively();
            tsOpen.TranslateToolStripItemsRecursively();
            tsOpenForPrinting.TranslateToolStripItemsRecursively();
            tsOpenForExport.TranslateToolStripItemsRecursively();
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

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }
    }
}
