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
using Chummer.Plugins;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using NLog;

namespace Chummer
{
    public partial class CharacterRoster : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private readonly FileSystemWatcher _watcherCharacterRosterFolderRawSaves;
        private readonly FileSystemWatcher _watcherCharacterRosterFolderCompressedSaves;
        private readonly DebuggableSemaphoreSlim _objCharacterRosterFolderWatcherSemaphore = new DebuggableSemaphoreSlim();
        private Task _tskMostRecentlyUsedsRefresh;
        private Task _tskWatchFolderRefresh;
        private CancellationTokenSource _objMostRecentlyUsedsRefreshCancellationTokenSource;
        private CancellationTokenSource _objWatchFolderRefreshCancellationTokenSource;
        private CancellationTokenSource _objUpdateCharacterCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        public CharacterRoster()
        {
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            InitializeComponent();
            tabCharacterText.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            if (!string.IsNullOrEmpty(GlobalSettings.CharacterRosterPath) && Directory.Exists(GlobalSettings.CharacterRosterPath))
            {
                _objCharacterRosterFolderWatcherSemaphore = new DebuggableSemaphoreSlim();
                _watcherCharacterRosterFolderRawSaves = new FileSystemWatcher(GlobalSettings.CharacterRosterPath, "*.chum5")
                    {
                        IncludeSubdirectories = true
                    };
                _watcherCharacterRosterFolderCompressedSaves = new FileSystemWatcher(GlobalSettings.CharacterRosterPath, "*.chum5lz")
                    {
                        IncludeSubdirectories = true
                    };

                Disposed += (sender, args) =>
                {
                    CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    objOldCancellationTokenSource = Interlocked.Exchange(ref _objWatchFolderRefreshCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCharacterCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    _objGenericFormClosingCancellationTokenSource.Dispose();
                    _watcherCharacterRosterFolderRawSaves.Dispose();
                    _watcherCharacterRosterFolderCompressedSaves.Dispose();
                    _objCharacterRosterFolderWatcherSemaphore.Dispose();
                };
            }
            else
            {
                Disposed += (sender, args) =>
                {
                    CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    objOldCancellationTokenSource = Interlocked.Exchange(ref _objWatchFolderRefreshCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCharacterCancellationTokenSource, null);
                    if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        objOldCancellationTokenSource.Cancel(false);
                        objOldCancellationTokenSource.Dispose();
                    }
                    _objGenericFormClosingCancellationTokenSource.Dispose();
                };
            }
        }

        public async Task SetMyEventHandlers(bool deleteThem = false, CancellationToken token = default)
        {
            ThreadSafeObservableCollection<CharacterShared>
                lstToProcess1 = Program.MainForm.OpenCharacterEditorForms;
            ThreadSafeObservableCollection<CharacterSheetViewer>
                lstToProcess2 = Program.MainForm.OpenCharacterSheetViewers;
            ThreadSafeObservableCollection<ExportCharacter>
                lstToProcess3 = Program.MainForm.OpenCharacterExportForms;
            if (!deleteThem)
            {
                if (lstToProcess1 != null)
                {
                    lstToProcess1.BeforeClearCollectionChangedAsync
                        += OpenCharacterEditorFormsOnBeforeClearCollectionChanged;
                    lstToProcess1.CollectionChangedAsync += OpenCharacterEditorFormsOnCollectionChanged;
                }
                if (lstToProcess2 != null)
                {
                    lstToProcess2.BeforeClearCollectionChangedAsync
                        += OpenCharacterSheetViewersOnBeforeClearCollectionChanged;
                    lstToProcess2.CollectionChangedAsync += OpenCharacterSheetViewersOnCollectionChanged;
                }
                if (lstToProcess3 != null)
                {
                    lstToProcess3.BeforeClearCollectionChangedAsync
                        += OpenCharacterExportFormsOnBeforeClearCollectionChanged;
                    lstToProcess3.CollectionChangedAsync += OpenCharacterExportFormsOnCollectionChanged;
                }
                GlobalSettings.MruChanged += RefreshMruLists;
                await treCharacterList.DoThreadSafeAsync(x =>
                {
                    x.ItemDrag += treCharacterList_OnDefaultItemDrag;
                    x.DragEnter += treCharacterList_OnDefaultDragEnter;
                    x.DragDrop += treCharacterList_OnDefaultDragDrop;
                    x.DragOver += treCharacterList_OnDefaultDragOver;
                    x.NodeMouseDoubleClick += treCharacterList_OnDefaultDoubleClick;
                }, token).ConfigureAwait(false);
                OnMyMouseDown += OnDefaultMouseDown;
                if (_watcherCharacterRosterFolderRawSaves != null)
                {
                    _watcherCharacterRosterFolderRawSaves.BeginInit();
                    _watcherCharacterRosterFolderRawSaves.Changed += RefreshSingleWatchNode;
                    _watcherCharacterRosterFolderRawSaves.Created += RefreshWatchList;
                    _watcherCharacterRosterFolderRawSaves.Deleted += DeleteSingleWatchNode;
                    _watcherCharacterRosterFolderRawSaves.Renamed += RefreshWatchList;
                    _watcherCharacterRosterFolderRawSaves.EnableRaisingEvents = true;
                    _watcherCharacterRosterFolderRawSaves.EndInit();
                    _watcherCharacterRosterFolderCompressedSaves.BeginInit();
                    _watcherCharacterRosterFolderCompressedSaves.Changed += RefreshSingleWatchNode;
                    _watcherCharacterRosterFolderCompressedSaves.Created += RefreshWatchList;
                    _watcherCharacterRosterFolderCompressedSaves.Deleted += DeleteSingleWatchNode;
                    _watcherCharacterRosterFolderCompressedSaves.Renamed += RefreshWatchList;
                    _watcherCharacterRosterFolderCompressedSaves.EnableRaisingEvents = true;
                    _watcherCharacterRosterFolderCompressedSaves.EndInit();
                }
            }
            else
            {
                if (lstToProcess1 != null)
                {
                    lstToProcess1.BeforeClearCollectionChangedAsync
                        -= OpenCharacterEditorFormsOnBeforeClearCollectionChanged;
                    lstToProcess1.CollectionChangedAsync -= OpenCharacterEditorFormsOnCollectionChanged;
                }
                if (lstToProcess2 != null)
                {
                    lstToProcess2.BeforeClearCollectionChangedAsync
                        -= OpenCharacterSheetViewersOnBeforeClearCollectionChanged;
                    lstToProcess2.CollectionChangedAsync -= OpenCharacterSheetViewersOnCollectionChanged;
                }
                if (lstToProcess3 != null)
                {
                    lstToProcess3.BeforeClearCollectionChangedAsync
                        -= OpenCharacterExportFormsOnBeforeClearCollectionChanged;
                    lstToProcess3.CollectionChangedAsync -= OpenCharacterExportFormsOnCollectionChanged;
                }
                GlobalSettings.MruChanged -= RefreshMruLists;
                await treCharacterList.DoThreadSafeAsync(x =>
                {
                    x.ItemDrag -= treCharacterList_OnDefaultItemDrag;
                    x.DragEnter -= treCharacterList_OnDefaultDragEnter;
                    x.DragDrop -= treCharacterList_OnDefaultDragDrop;
                    x.DragOver -= treCharacterList_OnDefaultDragOver;
                    x.NodeMouseDoubleClick -= treCharacterList_OnDefaultDoubleClick;
                }, token).ConfigureAwait(false);
                OnMyMouseDown = null;

                if (_watcherCharacterRosterFolderRawSaves != null)
                {
                    _watcherCharacterRosterFolderRawSaves.EnableRaisingEvents = false;
                    _watcherCharacterRosterFolderRawSaves.Changed -= RefreshSingleWatchNode;
                    _watcherCharacterRosterFolderRawSaves.Created -= RefreshWatchList;
                    _watcherCharacterRosterFolderRawSaves.Deleted -= DeleteSingleWatchNode;
                    _watcherCharacterRosterFolderRawSaves.Renamed -= RefreshWatchList;
                    _watcherCharacterRosterFolderCompressedSaves.EnableRaisingEvents = false;
                    _watcherCharacterRosterFolderCompressedSaves.Changed -= RefreshSingleWatchNode;
                    _watcherCharacterRosterFolderCompressedSaves.Created -= RefreshWatchList;
                    _watcherCharacterRosterFolderCompressedSaves.Deleted -= DeleteSingleWatchNode;
                    _watcherCharacterRosterFolderCompressedSaves.Renamed -= RefreshWatchList;
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

        private async void DeleteSingleWatchNode(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    // Use the watcher folder refresher's cancellation token so that the task gets canceled if we go for a full refresh
                    CancellationTokenSource objTemp = new CancellationTokenSource();
                    CancellationTokenSource objCurrent
                        = Interlocked.CompareExchange(ref _objWatchFolderRefreshCancellationTokenSource, objTemp, null);
                    if (objCurrent != null)
                    {
                        objTemp.Dispose();
                        objTemp = objCurrent;
                    }

                    await _objCharacterRosterFolderWatcherSemaphore.WaitAsync(objTemp.Token).ConfigureAwait(false);
                    try
                    {
                        if (_dicSavedCharacterCaches.TryRemove(e.FullPath, out CharacterCache objCacheToRemove))
                        {
                            await treCharacterList.DoThreadSafeAsync(x =>
                            {
                                foreach (TreeNode objNode in x.Nodes.OfType<TreeNode>()
                                                              .DeepWhere(y => y.Nodes.OfType<TreeNode>(),
                                                                         y => y.Tag == objCacheToRemove).ToList())
                                {
                                    objNode.Remove();
                                }
                            }, objTemp.Token).ConfigureAwait(false);
                            await objCacheToRemove.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        _objCharacterRosterFolderWatcherSemaphore.Release();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //swallow this, just in case
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshSingleWatchNode(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    // Use the watcher folder refresher's cancellation token so that the task gets canceled if we go for a full refresh
                    CancellationTokenSource objTemp = new CancellationTokenSource();
                    CancellationTokenSource objCurrentSource
                        = Interlocked.CompareExchange(ref _objWatchFolderRefreshCancellationTokenSource, objTemp, null);
                    if (objCurrentSource != null)
                    {
                        objTemp.Dispose();
                        objTemp = objCurrentSource;
                    }

                    CancellationToken objTokenToUse = objTemp.Token;
                    await _objCharacterRosterFolderWatcherSemaphore.WaitAsync(objTokenToUse).ConfigureAwait(false);
                    try
                    {
                        TreeNode objNewNode = await CacheCharacter(e.FullPath, true, objTokenToUse).ConfigureAwait(false);
                        if (objNewNode.Tag is CharacterCache objNewCache)
                        {
                            HashSet<CharacterCache> setCachesToDispose = new HashSet<CharacterCache>(2);
                            await treCharacterList.DoThreadSafeAsync(x =>
                            {
                                foreach (TreeNode objNode in x.Nodes.OfType<TreeNode>()
                                                              .DeepWhere(y => y.Nodes.OfType<TreeNode>(),
                                                                         y => y.Tag is CharacterCache z
                                                                              && z.FilePath == objNewCache.FilePath))
                                {
                                    objNode.Text = objNewNode.Text;
                                    objNode.ToolTipText = objNewNode.ToolTipText;
                                    objNode.ForeColor = objNewNode.ForeColor;
                                    if (objNode.Tag is CharacterCache objOldCache && !objOldCache.IsDisposed)
                                        setCachesToDispose.Add(objOldCache);
                                    objNode.Tag = objNewCache;
                                }
                            }, objTokenToUse).ConfigureAwait(false);
                            List<CharacterCache> lstToKeep = new List<CharacterCache>();
                            foreach (CharacterCache objCache in setCachesToDispose)
                            {
                                objTokenToUse.ThrowIfCancellationRequested();
                                if (!objCache.IsDisposed && _dicSavedCharacterCaches.ContainsKey(objCache.FilePath))
                                    lstToKeep.Add(objCache);
                            }
                            foreach (CharacterCache objCache in lstToKeep)
                            {
                                objTokenToUse.ThrowIfCancellationRequested();
                                setCachesToDispose.Remove(objCache);
                            }
                            foreach (CharacterCache objOldCache in setCachesToDispose)
                            {
                                await objOldCache.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        _objCharacterRosterFolderWatcherSemaphore.Release();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //swallow this, just in case
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void CharacterRoster_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    try
                    {
                        CharacterCache objSelectedCache
                            = await treCharacterList.DoThreadSafeFuncAsync(
                                    x => x.SelectedNode?.Tag, _objGenericToken).ConfigureAwait(false)
                                as CharacterCache;
                        await UpdateCharacter(objSelectedCache, _objGenericToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }

                    await SetMyEventHandlers(token: _objGenericToken).ConfigureAwait(false);
                    CancellationTokenSource objTemp = new CancellationTokenSource();
                    CancellationTokenSource objCurrent = Interlocked.CompareExchange(
                        ref _objMostRecentlyUsedsRefreshCancellationTokenSource,
                        objTemp, null);
                    if (objCurrent != null)
                    {
                        objTemp.Dispose();
                        objTemp = objCurrent;
                    }

                    Task tskNewRecentlyUsedsRefresh = LoadMruCharacters(true, objTemp.Token);
                    Task tskOldRecentlyUsedsRefresh = Interlocked.Exchange(ref _tskMostRecentlyUsedsRefresh, tskNewRecentlyUsedsRefresh);
                    if (tskOldRecentlyUsedsRefresh != null)
                    {
                        try
                        {
                            await tskOldRecentlyUsedsRefresh.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }

                    objTemp = new CancellationTokenSource();
                    objCurrent = Interlocked.CompareExchange(
                        ref _objWatchFolderRefreshCancellationTokenSource,
                        objTemp, null);
                    if (objCurrent != null)
                    {
                        objTemp.Dispose();
                        objTemp = objCurrent;
                    }

                    Task tskNewWatchFolderRefresh = LoadWatchFolderCharacters(objTemp.Token);
                    Task tskOldWatchFolderRefresh = Interlocked.Exchange(ref _tskWatchFolderRefresh, tskNewRecentlyUsedsRefresh);
                    if (tskOldWatchFolderRefresh != null)
                    {
                        try
                        {
                            await tskOldWatchFolderRefresh.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }

                    try
                    {
                        List<Task> lstTasks =
                                new List<Task>(2 + await Program.PluginLoader.MyPlugins.GetCountAsync(objTemp.Token)
                                    .ConfigureAwait(false))
                                {
                                    tskNewRecentlyUsedsRefresh,
                                    tskNewWatchFolderRefresh
                                };
                        foreach (IPlugin objPlugin in await Program.PluginLoader
                                     .GetMyActivePluginsAsync(objTemp.Token)
                                     .ConfigureAwait(false))
                        {
                            lstTasks.Add(RefreshPluginNodesAsync(objPlugin, objTemp.Token));
                        }
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                IsFinishedLoading = true;
            }
        }

        private int _intIsClosing;

        private async void CharacterRoster_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    if (Interlocked.CompareExchange(ref _intIsClosing, 1, 0) > 0) // Needed to prevent crashes on disposal
                        return;
                    CancellationTokenSource objTemp
                        = Interlocked.Exchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }

                    objTemp
                        = Interlocked.Exchange(ref _objWatchFolderRefreshCancellationTokenSource, null);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }

                    objTemp
                        = Interlocked.Exchange(ref _objUpdateCharacterCancellationTokenSource, null);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }

                    // Clear the mugshot image so that we don't get crashes from disposal ordering (image can get disposed before its picturebox does)
                    await picMugshot.DoThreadSafeAsync(x => x.Image = null, CancellationToken.None).ConfigureAwait(false);

                    await SetMyEventHandlers(true, _objGenericToken).ConfigureAwait(false);

                    await DisposeTagNodes(await treCharacterList.DoThreadSafeFuncAsync(x => x.Nodes, _objGenericToken).ConfigureAwait(false)).ConfigureAwait(false);

                    async ValueTask DisposeTagNodes(TreeNodeCollection lstNodes)
                    {
                        foreach (TreeNode nodNode in lstNodes)
                        {
                            if (nodNode.Tag is CharacterCache objCache)
                            {
                                nodNode.Tag = null;
                                if (!objCache.IsDisposed)
                                {
                                    if (_dicSavedCharacterCaches.TryRemove(objCache.FilePath, out CharacterCache objCacheToRemove)
                                        && !ReferenceEquals(objCacheToRemove, objCache)
                                        && !objCacheToRemove.IsDisposed)
                                        await objCacheToRemove.DisposeAsync().ConfigureAwait(false);
                                    await objCache.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            await DisposeTagNodes(nodNode.Nodes).ConfigureAwait(false);
                        }
                    }

                    _objGenericToken.ThrowIfCancellationRequested();
                    if (!_dicSavedCharacterCaches.IsEmpty)
                    {
                        List<CharacterCache> lstCaches = _dicSavedCharacterCaches.Values.ToList();
                        _dicSavedCharacterCaches.Clear();
                        foreach (CharacterCache objCache in lstCaches)
                        {
                            _objGenericToken.ThrowIfCancellationRequested();
                            if (!objCache.IsDisposed)
                                await objCache.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    try
                    {
                        Task tskTemp = Interlocked.Exchange(ref _tskMostRecentlyUsedsRefresh, null);
                        if (tskTemp != null)
                            await tskTemp.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }

                    try
                    {
                        Task tskTemp = Interlocked.Exchange(ref _tskWatchFolderRefresh, null);
                        if (tskTemp != null)
                            await tskTemp.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }

                    _objGenericFormClosingCancellationTokenSource.Cancel(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshWatchList(object sender, EventArgs e)
        {
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationToken innerToken = objNewSource.Token;
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objWatchFolderRefreshCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            Task tskTemp = Interlocked.Exchange(ref _tskWatchFolderRefresh, null);
            try
            {
                if (tskTemp != null)
                    await tskTemp.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            catch
            {
                _ = Interlocked.CompareExchange(ref _tskWatchFolderRefresh, tskTemp, null);
                Interlocked.CompareExchange(ref _objWatchFolderRefreshCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }

            if (this.IsNullOrDisposed())
            {
                Interlocked.CompareExchange(ref _objWatchFolderRefreshCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                return;
            }

            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), _objGenericToken).ConfigureAwait(false);
                try
                {
                    Task tskNewMostRecentlyUsedsRefresh = LoadWatchFolderCharacters(innerToken);
                    Task tskOldMostRecentlyUsedsRefresh = Interlocked.Exchange(ref _tskMostRecentlyUsedsRefresh, tskNewMostRecentlyUsedsRefresh);
                    if (tskOldMostRecentlyUsedsRefresh != null)
                    {
                        try
                        {
                            await tskOldMostRecentlyUsedsRefresh.ConfigureAwait(false);
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
                    try
                    {
                        await tskNewMostRecentlyUsedsRefresh.ConfigureAwait(false);
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
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }

                CharacterCache objSelectedCache = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, _objGenericToken).ConfigureAwait(false) as CharacterCache;
                await UpdateCharacter(objSelectedCache, _objGenericToken).ConfigureAwait(false);
                await PurgeUnusedCharacterCaches(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshMruLists(object sender, TextEventArgs e)
        {
            try
            {
                await RefreshMruLists(e?.Text, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public async Task RefreshMruLists(string strMruType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treCharacterList.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationToken objToken = objNewSource.Token;
            CancellationTokenSource objTemp
                = Interlocked.Exchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            token.ThrowIfCancellationRequested();

            Task tskTemp = Interlocked.Exchange(ref _tskMostRecentlyUsedsRefresh, null);
            try
            {
                if (tskTemp != null)
                    await tskTemp.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            catch
            {
                _ = Interlocked.CompareExchange(ref _tskMostRecentlyUsedsRefresh, tskTemp, null);
                Interlocked.CompareExchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }

            if (this.IsNullOrDisposed())
            {
                Interlocked.CompareExchange(ref _objMostRecentlyUsedsRefreshCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                return;
            }

            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
            try
            {
                Task tskNewRecentlyUsedsRefresh = LoadMruCharacters(strMruType != "mru", objToken);
                Task tskOldRecentlyUsedsRefresh = Interlocked.Exchange(ref _tskMostRecentlyUsedsRefresh, tskNewRecentlyUsedsRefresh);
                if (tskOldRecentlyUsedsRefresh != null)
                {
                    try
                    {
                        await tskOldRecentlyUsedsRefresh.ConfigureAwait(false);
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
                try
                {
                    await tskNewRecentlyUsedsRefresh.ConfigureAwait(false);
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
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            try
            {
                CharacterCache objSelectedCache = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token).ConfigureAwait(false) as CharacterCache;
                await UpdateCharacter(objSelectedCache, token).ConfigureAwait(false);
                await PurgeUnusedCharacterCaches(token).ConfigureAwait(false);
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

        private async Task OpenCharacterExportFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setToRefresh))
                {
                    bool blnRefreshMru = false;
                    // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                    foreach (ExportCharacter objForm in e.OldItems)
                    {
                        token.ThrowIfCancellationRequested();
                        string strFile = objForm.CharacterObject.FileName;
                        setToRefresh.Add(strFile);
                        if (await GlobalSettings.FavoriteCharacters.ContainsAsync(
                                strFile, token: token).ConfigureAwait(false))
                            continue;
                        if (await GlobalSettings.MostRecentlyUsedCharacters.ContainsAsync(
                                strFile, token: token).ConfigureAwait(false))
                            continue;
                        blnRefreshMru = true;
                        break;
                    }

                    if (blnRefreshMru)
                        await RefreshMruLists("mru", token).ConfigureAwait(false);
                    await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        private async Task OpenCharacterExportFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setToRefresh))
                        {
                            foreach (ExportCharacter objForm in e.NewItems)
                            {
                                token.ThrowIfCancellationRequested();
                                setToRefresh.Add(objForm.CharacterObject.FileName);
                            }

                            await RefreshNodeTexts(token: token).ConfigureAwait(false);
                        }

                        break;

                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Remove:
                        {
                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> setToRefresh))
                            {
                                bool blnRefreshMru = false;
                                // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                                foreach (ExportCharacter objForm in e.OldItems)
                                {
                                    token.ThrowIfCancellationRequested();
                                    string strFile = objForm.CharacterObject.FileName;
                                    setToRefresh.Add(strFile);
                                    if (await GlobalSettings.FavoriteCharacters.ContainsAsync(
                                            strFile, token: token).ConfigureAwait(false))
                                        continue;
                                    if (await GlobalSettings.MostRecentlyUsedCharacters.ContainsAsync(
                                            strFile, token: token).ConfigureAwait(false))
                                        continue;
                                    blnRefreshMru = true;
                                    break;
                                }

                                if (blnRefreshMru)
                                    await RefreshMruLists("mru", token).ConfigureAwait(false);
                                await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        await RefreshMruLists(string.Empty, token).ConfigureAwait(false);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        private async Task OpenCharacterSheetViewersOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setToRefresh))
                {
                    bool blnRefreshMru = false;
                    using (new FetchSafelyFromPool<HashSet<string>>(
                               Utils.StringHashSetPool, out HashSet<string> setCharacters))
                    {
                        // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                        foreach (CharacterSheetViewer objForm in e.OldItems)
                        {
                            token.ThrowIfCancellationRequested();
                            setCharacters.Clear();
                            setCharacters.AddRange(objForm.CharacterObjects.Select(x => x.FileName));
                            setToRefresh.AddRange(setCharacters);
                            setCharacters.ExceptWith(GlobalSettings.FavoriteCharacters);
                            setCharacters.ExceptWith(GlobalSettings.MostRecentlyUsedCharacters);
                            if (setCharacters.Count > 0)
                            {
                                blnRefreshMru = true;
                                break;
                            }
                        }
                    }

                    if (blnRefreshMru)
                        await RefreshMruLists("mru", token).ConfigureAwait(false);
                    await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        private async Task OpenCharacterSheetViewersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setToRefresh))
                        {
                            foreach (CharacterSheetViewer objForm in e.NewItems)
                            {
                                token.ThrowIfCancellationRequested();
                                setToRefresh.AddRange(objForm.CharacterObjects.Select(x => x.FileName));
                            }
                            await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                        }

                        break;

                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Remove:
                    {
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setToRefresh))
                        {
                            bool blnRefreshMru = false;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setCharacters))
                            {
                                // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                                foreach (CharacterSheetViewer objForm in e.OldItems)
                                {
                                    token.ThrowIfCancellationRequested();
                                    setCharacters.Clear();
                                    setCharacters.AddRange(objForm.CharacterObjects.Select(x => x.FileName));
                                    setToRefresh.AddRange(setCharacters);
                                    setCharacters.ExceptWith(GlobalSettings.FavoriteCharacters);
                                    setCharacters.ExceptWith(GlobalSettings.MostRecentlyUsedCharacters);
                                    if (setCharacters.Count > 0)
                                    {
                                        blnRefreshMru = true;
                                        break;
                                    }
                                }
                            }

                            if (blnRefreshMru)
                                await RefreshMruLists("mru", token).ConfigureAwait(false);
                            await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        await RefreshMruLists(string.Empty, token).ConfigureAwait(false);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        private async Task OpenCharacterEditorFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setToRefresh))
                {
                    bool blnRefreshMru = false;
                    // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                    foreach (CharacterShared objForm in e.OldItems)
                    {
                        token.ThrowIfCancellationRequested();
                        string strFile = objForm.CharacterObject.FileName;
                        setToRefresh.Add(strFile);
                        if (await GlobalSettings.FavoriteCharacters.ContainsAsync(
                                strFile, token: token).ConfigureAwait(false))
                            continue;
                        if (await GlobalSettings.MostRecentlyUsedCharacters.ContainsAsync(
                                strFile, token: token).ConfigureAwait(false))
                            continue;
                        blnRefreshMru = true;
                        break;
                    }

                    if (blnRefreshMru)
                        await RefreshMruLists("mru", token).ConfigureAwait(false);
                    await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        private async Task OpenCharacterEditorFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setToRefresh))
                        {
                            foreach (CharacterShared objForm in e.NewItems)
                            {
                                token.ThrowIfCancellationRequested();
                                setToRefresh.Add(objForm.CharacterObject.FileName);
                            }

                            await RefreshNodeTexts(token: token).ConfigureAwait(false);
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Remove:
                    {
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setToRefresh))
                        {
                            bool blnRefreshMru = false;
                            // Because the Recent Characters list can have characters listed that aren't in either MRU, refresh it if we are moving or removing any such character
                            foreach (CharacterShared objForm in e.OldItems)
                            {
                                token.ThrowIfCancellationRequested();
                                string strFile = objForm.CharacterObject.FileName;
                                setToRefresh.Add(strFile);
                                if (await GlobalSettings.FavoriteCharacters.ContainsAsync(
                                                            objForm.CharacterObject.FileName, token: token)
                                                        .ConfigureAwait(false))
                                    continue;
                                if (await GlobalSettings.MostRecentlyUsedCharacters.ContainsAsync(
                                                            objForm.CharacterObject.FileName, token: token)
                                                        .ConfigureAwait(false))
                                    continue;
                                blnRefreshMru = true;
                                break;
                            }

                            if (blnRefreshMru)
                                await RefreshMruLists("mru", token).ConfigureAwait(false);
                            await RefreshNodeTexts(setToRefresh, token).ConfigureAwait(false);
                        }
                    }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        await RefreshMruLists(string.Empty, token).ConfigureAwait(false);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        public async Task RefreshNodeTexts(ICollection<string> lstNames = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (treCharacterList.IsNullOrDisposed())
                return;
            if (!IsFinishedLoading)
                return;

            string strErrorPrefix
                = await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false)
                  + await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false)
                  + Environment.NewLine;

            Dictionary<TreeNode, string> dicNodeNames = new Dictionary<TreeNode, string>();
            foreach (TreeNode objCharacterNode in await treCharacterList
                                                        .DoThreadSafeFuncAsync(
                                                            x => x.Nodes.Cast<TreeNode>()
                                                                  .DeepWhere(y => y.Nodes.Cast<TreeNode>(),
                                                                             y => y.Tag is CharacterCache z
                                                                                 && lstNames?.Contains(z.FilePath) != false)
                                                                  .ToList(),
                                                            token: token).ConfigureAwait(false))
            {
                if (!(objCharacterNode.Tag is CharacterCache objCache))
                    continue;
                dicNodeNames.Add(objCharacterNode,
                                 await objCache.CalculatedNameAsync(token: token).ConfigureAwait(false));
            }

            await treCharacterList.DoThreadSafeAsync(() =>
            {
                foreach (KeyValuePair<TreeNode, string> kvpNode in dicNodeNames)
                {
                    TreeNode objCharacterNode = kvpNode.Key;
                    if (!(objCharacterNode.Tag is CharacterCache objCache))
                        continue;
                    objCharacterNode.Text = kvpNode.Value;
                    string strTooltip = string.Empty;
                    if (!string.IsNullOrEmpty(objCache.FilePath))
                    {
                        strTooltip = objCache.FilePath.Replace(Utils.GetStartupPath,
                                                               '<' + Application.ProductName + '>');
                    }

                    if (!string.IsNullOrEmpty(objCache.ErrorText))
                    {
                        objCharacterNode.ForeColor = ColorManager.ErrorColor;
                        if (!string.IsNullOrEmpty(objCache.FilePath))
                            strTooltip += Environment.NewLine + Environment.NewLine;
                        strTooltip += strErrorPrefix + objCache.ErrorText;
                    }
                    else
                        objCharacterNode.ForeColor = ColorManager.WindowText;

                    objCharacterNode.ToolTipText = strTooltip;
                }
            }, token).ConfigureAwait(false);
        }

        private async Task LoadMruCharacters(bool blnRefreshFavorites, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (treCharacterList.IsNullOrDisposed())
                return;

            List<string> lstFavorites = (await GlobalSettings.FavoriteCharacters.ToArrayAsync(token).ConfigureAwait(false)).ToList();
            bool blnAddFavoriteNode = false;
            TreeNode objFavoriteNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.FindNode("Favorite", false), token).ConfigureAwait(false);
            if (objFavoriteNode == null && blnRefreshFavorites)
            {
                objFavoriteNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_FavoriteCharacters", token: token).ConfigureAwait(false))
                    {Tag = "Favorite"};
                blnAddFavoriteNode = true;
            }

            token.ThrowIfCancellationRequested();

            bool blnAddRecentNode = false;
            List<string> lstRecents = (await GlobalSettings.MostRecentlyUsedCharacters.ToArrayAsync(token).ConfigureAwait(false)).ToList();
            // Add any characters that are open to the displayed list so we can have more than 10 characters listed
            foreach (string strFile in Program.MainForm.OpenFormsWithCharacters.SelectMany(
                         x => x.CharacterObjects).Select(x => x.FileName))
            {
                // Make sure we're not loading a character that was already loaded by the MRU list.
                if (!lstFavorites.Contains(strFile) && !lstRecents.Contains(strFile))
                    lstRecents.Add(strFile);
            }
            foreach (string strFavorite in lstFavorites)
                lstRecents.Remove(strFavorite);
            if (!blnRefreshFavorites)
                lstFavorites.Clear();
            TreeNode objRecentNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.FindNode("Recent", false), token).ConfigureAwait(false);
            if (objRecentNode == null && lstRecents.Count > 0)
            {
                objRecentNode = new TreeNode(await LanguageManager.GetStringAsync("Treenode_Roster_RecentCharacters", token: token).ConfigureAwait(false))
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
                        = Task.Run(() => CacheCharacter(lstFavorites[iLocal], token: token), token);
                }

                for (int i = 0; i < intRecentsCount; ++i)
                {
                    int iLocal = i;
                    atskCachingTasks[intFavoritesCount + i]
                        = Task.Run(() => CacheCharacter(lstRecents[iLocal], token: token), token);
                }

                try
                {
                    await Task.WhenAll(atskCachingTasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }

                if (lstFavoritesNodes != null)
                {
                    for (int i = 0; i < intFavoritesCount; ++i)
                    {
                        lstFavoritesNodes[i] = await atskCachingTasks[i].ConfigureAwait(false);
                    }

                    if (objFavoriteNode != null)
                    {
                        foreach (TreeNode objNode in lstFavoritesNodes)
                        {
                            token.ThrowIfCancellationRequested();
                            if (objNode == null)
                                continue;
                            TreeView treFavoriteNode = objFavoriteNode.TreeView;
                            if (treFavoriteNode != null)
                            {
                                if (treFavoriteNode.IsNullOrDisposed())
                                    continue;
                                await treFavoriteNode.DoThreadSafeAsync(
                                    () => objFavoriteNode.Nodes.Add(objNode), token).ConfigureAwait(false);
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
                        lstRecentsNodes[i] = await atskCachingTasks[intFavoritesCount + i].ConfigureAwait(false);
                    }

                    if (objRecentNode != null)
                    {
                        foreach (TreeNode objNode in lstRecentsNodes)
                        {
                            token.ThrowIfCancellationRequested();
                            if (objNode == null)
                                continue;
                            TreeView treRecentNode = objRecentNode.TreeView;
                            if (treRecentNode != null)
                            {
                                if (treRecentNode.IsNullOrDisposed())
                                    continue;
                                await treRecentNode.DoThreadSafeAsync(
                                    () => objRecentNode.Nodes.Add(objNode), token).ConfigureAwait(false);
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
                try
                {
                    if (blnRefreshFavorites && objFavoriteNode != null)
                    {
                        if (blnAddFavoriteNode)
                        {
                            treList.Nodes.Insert(0, objFavoriteNode);
                        }
                        else if (lstFavoritesNodes != null)
                        {
                            try
                            {
                                objFavoriteNode.Nodes.Clear();
                                foreach (TreeNode objNode in lstFavoritesNodes)
                                {
                                    if (objNode != null)
                                        objFavoriteNode.Nodes.Add(objNode);
                                }
                            }
                            catch (ObjectDisposedException e)
                            {
                                //just swallow this
                                Log.Trace(e, "ObjectDisposedException can be ignored here.");
                            }
                        }
                        objFavoriteNode.ExpandAll();
                    }

                    if (objRecentNode != null)
                    {
                        if (blnAddRecentNode)
                        {
                            treList.Nodes.Insert((objFavoriteNode != null).ToInt32(), objRecentNode);
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
                }
                finally
                {
                    treList.ResumeLayout();
                }
            }, token).ConfigureAwait(false);
        }

        private async Task LoadWatchFolderCharacters(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (treCharacterList.IsNullOrDisposed())
                return;

            Dictionary<string, string> dicWatch = null;
            string strErrorText = string.Empty;
            if (!string.IsNullOrEmpty(GlobalSettings.CharacterRosterPath) && Directory.Exists(GlobalSettings.CharacterRosterPath))
            {
                dicWatch = new Dictionary<string, string>(byte.MaxValue);
                try
                {
                    foreach (string strFile in Directory
                                               .EnumerateFiles(GlobalSettings.CharacterRosterPath, "*.chum5",
                                                               SearchOption.AllDirectories)
                                               .Concat(Directory.EnumerateFiles(
                                                           GlobalSettings.CharacterRosterPath, "*.chum5lz",
                                                           SearchOption.AllDirectories)))
                    {
                        token.ThrowIfCancellationRequested();

                        FileInfo objInfo = new FileInfo(strFile);
                        string strDirectoryFullName = objInfo.Directory?.FullName ?? string.Empty;
                        if (string.IsNullOrEmpty(strDirectoryFullName)
                            || strDirectoryFullName == GlobalSettings.CharacterRosterPath)
                        {
                            dicWatch.Add(strFile, "Watch");
                            continue;
                        }

                        string strNewParent
                            = strDirectoryFullName.Replace(
                                GlobalSettings.CharacterRosterPath + Path.DirectorySeparatorChar, string.Empty);
                        dicWatch.Add(strFile, strNewParent);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Throw cancellations because we expect to handle them in whatever awaits this task
                    throw;
                }
                catch (UnauthorizedAccessException e)
                {
                    // We do not have sufficient privileges for this directory
                    strErrorText = e.Message;
                    dicWatch.Clear();
                }
                catch (Exception e)
                {
                    // We had some other issue while trying to load the character roster, so log it
                    Log.Warn(e);
                    strErrorText = e.Message;
                    dicWatch.Clear();
                }
            }
            bool blnAddWatchNode = !string.IsNullOrEmpty(strErrorText) || dicWatch?.Count > 0;
            TreeNode objWatchNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.FindNode("Watch", false), token).ConfigureAwait(false);
            if (blnAddWatchNode)
            {
                string strWatchText = await LanguageManager.GetStringAsync("Treenode_Roster_WatchFolder", token: token)
                                                           .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strErrorText))
                    strWatchText
                        += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false)
                           + '(' + await LanguageManager.GetStringAsync("String_Error", token: token)
                                                        .ConfigureAwait(false) + ')';
                if (objWatchNode != null)
                {
                    await treCharacterList.DoThreadSafeAsync(() =>
                                          {
                                              objWatchNode.Nodes.Clear();
                                              if (string.IsNullOrEmpty(strErrorText))
                                              {
                                                  objWatchNode.ForeColor = ColorManager.WindowText;
                                                  objWatchNode.ToolTipText = string.Empty;
                                              }
                                              else
                                              {
                                                  objWatchNode.ForeColor = ColorManager.ErrorColor;
                                                  objWatchNode.ToolTipText = strErrorText;
                                              }
                                          }, token)
                                          .ConfigureAwait(false);
                }
                else
                {
                    objWatchNode = new TreeNode(strWatchText) {Tag = "Watch"};
                    if (!string.IsNullOrEmpty(strErrorText))
                    {
                        objWatchNode.ForeColor = ColorManager.ErrorColor;
                        objWatchNode.ToolTipText = strErrorText;
                    }
                }
            }
            else if (objWatchNode != null)
                await treCharacterList.DoThreadSafeAsync(() => objWatchNode.Remove(), token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            if (!string.IsNullOrEmpty(strErrorText) || objWatchNode == null || !blnAddWatchNode || dicWatch == null || dicWatch.Count == 0)
                return;

            Dictionary<TreeNode, string> dicWatchNodes = new Dictionary<TreeNode, string>(dicWatch.Count);
            List<Task<TreeNode>> lstCachingTasks = new List<Task<TreeNode>>(Utils.MaxParallelBatchSize);
            int intCounter = 0;
            foreach (string strKey in dicWatch.Keys)
            {
                lstCachingTasks.Add(Task.Run(() => CacheCharacter(strKey, token: token), token));
                if (++intCounter != Utils.MaxParallelBatchSize)
                    continue;
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(lstCachingTasks).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                foreach (Task<TreeNode> tskCachingTask in lstCachingTasks)
                {
                    TreeNode objNode = await tskCachingTask.ConfigureAwait(false);
                    if (objNode.Tag is CharacterCache objCache)
                        dicWatchNodes.Add(objNode, dicWatch[objCache.FilePath]);
                    token.ThrowIfCancellationRequested();
                }
                lstCachingTasks.Clear();
                intCounter = 0;
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstCachingTasks).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            foreach (Task<TreeNode> tskCachingTask in lstCachingTasks)
            {
                TreeNode objNode = await tskCachingTask.ConfigureAwait(false);
                if (objNode.Tag is CharacterCache objCache)
                    dicWatchNodes.Add(objNode, dicWatch[objCache.FilePath]);
                token.ThrowIfCancellationRequested();
            }

            foreach (string s in new SortedSet<string>(dicWatchNodes.Values))
            {
                token.ThrowIfCancellationRequested();
                if (s == "Watch")
                    continue;
                if (objWatchNode.TreeView != null)
                {
                    if (objWatchNode.TreeView.IsDisposed)
                        continue;
                    await objWatchNode.TreeView.DoThreadSafeAsync(
                        () => objWatchNode.Nodes.Add(new TreeNode(s) {Tag = s}), token).ConfigureAwait(false);
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
                        await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key), token).ConfigureAwait(false);
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
                            await objWatchNode.TreeView.DoThreadSafeAsync(() => objWatchNode.Nodes.Add(kvtNode.Key), token).ConfigureAwait(false);
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
                x.SuspendLayout();
                if (objWatchNode != null)
                {
                    if (objWatchNode.TreeView == null)
                    {
                        TreeNode objFavoriteNode = x.FindNode("Favorite", false);
                        TreeNode objRecentNode = x.FindNode("Recent", false);
                        if (objFavoriteNode != null && objRecentNode != null)
                            x.Nodes.Insert(2, objWatchNode);
                        else if (objFavoriteNode != null || objRecentNode != null)
                            x.Nodes.Insert(1, objWatchNode);
                        else
                            x.Nodes.Insert(0, objWatchNode);
                    }
                    objWatchNode.ExpandAll();
                }
                x.ResumeLayout();
            }, token).ConfigureAwait(false);
        }

        public Task RefreshPluginNodesAsync(IPlugin objPluginToRefresh, CancellationToken objToken = default)
        {
            if (objPluginToRefresh == null)
                throw new ArgumentNullException(nameof(objPluginToRefresh));
            return RefreshPluginNodesInner(objToken); // Split up this way so that the parameter check happens synchronously

            async Task RefreshPluginNodesInner(CancellationToken token = default)
            {
                token.ThrowIfCancellationRequested();
                int intNodeOffset
                    = (await Program.PluginLoader.GetMyActivePluginsAsync(token).ConfigureAwait(false)).IndexOf(
                        objPluginToRefresh);
                if (intNodeOffset >= 0)
                {
                    Log.Info("Starting new Task to get CharacterRosterTreeNodes for plugin:" + objPluginToRefresh);
                    List<TreeNode> lstNodes =
                        (await objPluginToRefresh.GetCharacterRosterTreeNode(this, true, token).ConfigureAwait(false))?.ToList();
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
                                 .FirstOrDefault(y => y.Text == strNodeText && y.Tag == objNodeTag), token).ConfigureAwait(false);
                            try
                            {
                                int i1 = i;
                                await treCharacterList.DoThreadSafeAsync(treList =>
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (objExistingNode != null)
                                    {
                                        treList.Nodes.Remove(objExistingNode);
                                        token.ThrowIfCancellationRequested();
                                    }

                                    if (node.Nodes.Count > 0 || !string.IsNullOrEmpty(node.ToolTipText)
                                                             || objNodeTag != null)
                                    {
                                        if (treList.Nodes.ContainsKey(node.Name))
                                            treList.Nodes.RemoveByKey(node.Name);
                                        TreeNode objFavoriteNode = treList.FindNode("Favorite", false);
                                        TreeNode objRecentNode = treList.FindNode("Recent", false);
                                        TreeNode objWatchNode = treList.FindNode("Watch", false);
                                        token.ThrowIfCancellationRequested();
                                        if (objFavoriteNode != null && objRecentNode != null
                                                                    && objWatchNode != null)
                                            treList.Nodes.Insert(i1 + intNodeOffset + 3, node);
                                        else if (objFavoriteNode != null || objRecentNode != null
                                                                         || objWatchNode != null)
                                        {
                                            if ((objFavoriteNode != null && objRecentNode != null) ||
                                                (objFavoriteNode != null && objWatchNode != null) ||
                                                (objRecentNode != null && objWatchNode != null))
                                                treList.Nodes.Insert(i1 + intNodeOffset + 2, node);
                                            else
                                                treList.Nodes.Insert(i1 + intNodeOffset + 1, node);
                                        }
                                        else
                                            treList.Nodes.Insert(i1 + intNodeOffset, node);
                                    }

                                    node.Expand();
                                }, token).ConfigureAwait(false);
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
                }
                else
                {
                    Utils.BreakIfDebug();
                }
            }
        }

        private readonly ConcurrentDictionary<string, CharacterCache> _dicSavedCharacterCaches = new ConcurrentDictionary<string, CharacterCache>();

        /// <summary>
        /// Remove all character caches from the cached dictionary that are not present in any of the form's lists (and are therefore unnecessary).
        /// </summary>
        private async Task PurgeUnusedCharacterCaches(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            foreach (KeyValuePair<string, CharacterCache> kvpCache in _dicSavedCharacterCaches.ToList())
            {
                token.ThrowIfCancellationRequested();
                CharacterCache objCache = kvpCache.Value;
                if (await treCharacterList.DoThreadSafeFuncAsync(x => x.FindNodeByTag(objCache), token).ConfigureAwait(false) != null)
                    continue;
                token.ThrowIfCancellationRequested();
                if (!_dicSavedCharacterCaches.TryRemove(kvpCache.Key, out CharacterCache objCacheToDelete) || !ReferenceEquals(objCacheToDelete, objCache))
                    continue;
                if (!objCacheToDelete.IsDisposed)
                    await objCacheToDelete.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates a character cache, which prevents us from repeatedly loading XmlNodes or caching a full character.
        /// The cache is then saved in a dictionary to prevent us from storing duplicate image data in memory (which can get expensive!)
        /// </summary>
        private async Task<TreeNode> CacheCharacter(string strFile, bool blnForceRecache = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CharacterCache objCache = null;
            if (_intIsClosing == 0)
            {
                try
                {
                    CharacterCache objTemp = null;
                    try
                    {
                        AsyncLazy<CharacterCache> objGeneratedCache
                            = new AsyncLazy<CharacterCache>(() => CharacterCache.CreateFromFileAsync(strFile, token),
                                                            Utils.JoinableTaskFactory);
                        if (blnForceRecache)
                        {
                            CharacterCache objToDispose = null;
                            objCache = await _dicSavedCharacterCaches
                                             .AddOrUpdateAsync(
                                                 strFile,
                                                 async x => objTemp = await objGeneratedCache.GetValueAsync(token)
                                                     .ConfigureAwait(false),
                                                 async (x, y) =>
                                                 {
                                                     objToDispose = y;
                                                     return objTemp = await objGeneratedCache.GetValueAsync(token).ConfigureAwait(false);
                                                 }, token)
                                             .ConfigureAwait(false);
                            if (objToDispose != null && !ReferenceEquals(objToDispose, objCache) && !objToDispose.IsDisposed)
                                await objToDispose.DisposeAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            objCache = await _dicSavedCharacterCaches
                                             .GetOrAddAsync(
                                                 strFile,
                                                 async x => objTemp = await objGeneratedCache.GetValueAsync(token).ConfigureAwait(false),
                                                 token)
                                             .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        if (objTemp != null && !ReferenceEquals(objCache, objTemp))
                        {
                            await objTemp.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // We shouldn't be caching characters if we've already disposed ourselves, so if you break here,
                    // something went wrong (but not fatally so, which is why the exception is handled)
                    Utils.BreakIfDebug();
                    if (objCache == null)
                        objCache = await CharacterCache.CreateFromFileAsync(strFile, token).ConfigureAwait(false);
                }
            }
            else
                objCache = await CharacterCache.CreateFromFileAsync(strFile, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (objCache == null)
            {
                return new TreeNode
                {
                    Text = await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false),
                    ForeColor = ColorManager.ErrorColor
                };
            }

            token.ThrowIfCancellationRequested();
            TreeNode objNode = new TreeNode
            {
                Text = await objCache.CalculatedNameAsync(token: token).ConfigureAwait(false),
                ToolTipText = await objCache.FilePath.CheapReplaceAsync(Utils.GetStartupPath,
                                                                        () => '<' + Application.ProductName + '>', token: token).ConfigureAwait(false),
                Tag = objCache
            };
            if (!string.IsNullOrEmpty(objCache.ErrorText))
            {
                objNode.ForeColor = ColorManager.ErrorColor;
                if (!string.IsNullOrEmpty(objNode.ToolTipText))
                    objNode.ToolTipText += Environment.NewLine + Environment.NewLine;
                objNode.ToolTipText += await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false) +
                                       await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false) + Environment.NewLine +
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
            if (await this.DoThreadSafeFuncAsync(x => x.IsNullOrDisposed(), token).ConfigureAwait(false)) // Safety check for external calls
                return;
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateCharacterCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    await tlpRight.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (objCache != null)
                        {
                            string strUnknown = await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            string strTemp1 = await objCache.Description.RtfToPlainTextAsync(token).ConfigureAwait(false);
                            await txtCharacterBio.DoThreadSafeAsync(x => x.Text = strTemp1, token).ConfigureAwait(false);
                            string strTemp2 = await objCache.Background.RtfToPlainTextAsync(token).ConfigureAwait(false);
                            await txtCharacterBackground.DoThreadSafeAsync(x => x.Text = strTemp2, token).ConfigureAwait(false);
                            string strTemp3 = await objCache.CharacterNotes.RtfToPlainTextAsync(token).ConfigureAwait(false);
                            await txtCharacterNotes.DoThreadSafeAsync(x => x.Text = strTemp3, token).ConfigureAwait(false);
                            string strTemp4 = await objCache.GameNotes.RtfToPlainTextAsync(token).ConfigureAwait(false);
                            await txtGameNotes.DoThreadSafeAsync(x => x.Text = strTemp4, token).ConfigureAwait(false);
                            string strTemp5 = await objCache.Concept.RtfToPlainTextAsync(token).ConfigureAwait(false);
                            await txtCharacterConcept.DoThreadSafeAsync(x => x.Text = strTemp5, token).ConfigureAwait(false);
                            string strText = objCache.Karma;
                            if (string.IsNullOrEmpty(strText) || strText == 0.ToString(GlobalSettings.CultureInfo))
                                strText = await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false);
                            await lblCareerKarma.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                            await lblPlayerName.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.PlayerName;
                                if (string.IsNullOrEmpty(x.Text))
                                    x.Text = strUnknown;
                            }, token).ConfigureAwait(false);
                            await lblCharacterName.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.CharacterName;
                                if (string.IsNullOrEmpty(x.Text))
                                    x.Text = strUnknown;
                            }, token).ConfigureAwait(false);
                            await lblCharacterAlias.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.CharacterAlias;
                                if (string.IsNullOrEmpty(x.Text))
                                    x.Text = strUnknown;
                            }, token).ConfigureAwait(false);
                            await lblEssence.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.Essence;
                                if (string.IsNullOrEmpty(x.Text))
                                    x.Text = strUnknown;
                            }, token).ConfigureAwait(false);
                            string strText2 = objCache.FileName;
                            if (string.IsNullOrEmpty(strText2))
                                strText2 = await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token).ConfigureAwait(false);
                            await lblFilePath.DoThreadSafeAsync(x => x.Text = strText2, token).ConfigureAwait(false);
                            await lblSettings.DoThreadSafeAsync(x =>
                            {
                                x.Text = objCache.SettingsFile;
                                if (string.IsNullOrEmpty(x.Text))
                                    x.Text = strUnknown;
                            }, token).ConfigureAwait(false);
                            await lblFilePath.SetToolTipAsync(
                                await objCache.FilePath.CheapReplaceAsync(Utils.GetStartupPath,
                                                                          () => '<' + Application.ProductName + '>', token: token).ConfigureAwait(false),
                                token).ConfigureAwait(false);
                            await picMugshot.DoThreadSafeAsync(x => x.Image = objCache.Mugshot, token).ConfigureAwait(false);
                            // Populate character information fields.
                            if (objCache.Metatype != null)
                            {
                                XPathNavigator objMetatypeDoc
                                    = await XmlManager.LoadXPathAsync("metatypes.xml", token: token).ConfigureAwait(false);
                                XPathNavigator objMetatypeNode
                                    = objMetatypeDoc.SelectSingleNode(
                                        "/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + ']');
                                if (objMetatypeNode == null)
                                {
                                    objMetatypeDoc = await XmlManager.LoadXPathAsync("critters.xml", token: token).ConfigureAwait(false);
                                    objMetatypeNode = objMetatypeDoc.SelectSingleNode(
                                        "/chummer/metatypes/metatype[name = " + objCache.Metatype?.CleanXPath() + ']');
                                }

                                token.ThrowIfCancellationRequested();
                                string strMetatype = objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objCache.Metatype;

                                if (!string.IsNullOrEmpty(objCache.Metavariant) && objCache.Metavariant != "None")
                                {
                                    objMetatypeNode = objMetatypeNode?.SelectSingleNode(
                                        "metavariants/metavariant[name = " + objCache.Metavariant.CleanXPath() + ']');

                                    strMetatype += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                                        + (objMetatypeNode?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? objCache.Metavariant) + ')';
                                }

                                await lblMetatype.DoThreadSafeAsync(x => x.Text = strMetatype, token).ConfigureAwait(false);
                            }
                            else
                            {
                                string strTemp = await LanguageManager.GetStringAsync("String_MetatypeLoadError", token: token).ConfigureAwait(false);
                                await lblMetatype.DoThreadSafeAsync(x => x.Text = strTemp, token).ConfigureAwait(false);
                            }

                            await tabCharacterText.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(objCache.ErrorText))
                            {
                                await txtCharacterBio.DoThreadSafeAsync(x =>
                                {
                                    x.Text = objCache.ErrorText;
                                    x.ForeColor = ColorManager.ErrorColor;
                                    x.BringToFront();
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await txtCharacterBio.DoThreadSafeAsync(x => x.ForeColor = ColorManager.WindowText, token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await tabCharacterText.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                            await txtCharacterBio.DoThreadSafeAsync(x => x.Clear(), token).ConfigureAwait(false);
                            await txtCharacterBackground.DoThreadSafeAsync(x => x.Clear(), token).ConfigureAwait(false);
                            await txtCharacterNotes.DoThreadSafeAsync(x => x.Clear(), token).ConfigureAwait(false);
                            await txtGameNotes.DoThreadSafeAsync(x => x.Clear(), token).ConfigureAwait(false);
                            await txtCharacterConcept.DoThreadSafeAsync(x => x.Clear(), token).ConfigureAwait(false);
                            await lblCareerKarma.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblMetatype.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblPlayerName.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblCharacterName.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblCharacterAlias.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblEssence.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblFilePath.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await lblFilePath.SetToolTipAsync(string.Empty, token).ConfigureAwait(false);
                            await lblSettings.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                            await picMugshot.DoThreadSafeAsync(x => x.Image = null, token).ConfigureAwait(false);
                        }

                        await lblCareerKarmaLabel.DoThreadSafeAsync(
                            x => x.Visible = !string.IsNullOrEmpty(lblCareerKarma.Text), token).ConfigureAwait(false);
                        await lblMetatypeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblMetatype.Text),
                                                                 token).ConfigureAwait(false);
                        await lblPlayerNameLabel.DoThreadSafeAsync(
                            x => x.Visible = !string.IsNullOrEmpty(lblPlayerName.Text), token).ConfigureAwait(false);
                        await lblCharacterNameLabel.DoThreadSafeAsync(
                            x => x.Visible = !string.IsNullOrEmpty(lblCharacterName.Text), token).ConfigureAwait(false);
                        await lblCharacterAliasLabel.DoThreadSafeAsync(
                            x => x.Visible = !string.IsNullOrEmpty(lblCharacterAlias.Text), token).ConfigureAwait(false);
                        await lblEssenceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblEssence.Text),
                                                                token).ConfigureAwait(false);
                        await lblFilePathLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblFilePath.Text),
                                                                 token).ConfigureAwait(false);
                        await lblSettingsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(lblSettings.Text),
                                                                 token).ConfigureAwait(false);
                        await ProcessMugshotSizeMode(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await tlpRight.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #region Form Methods

        private async void treCharacterList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode objSelectedNode
                    = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken).ConfigureAwait(false);
                if (objSelectedNode == null)
                    return;
                CharacterCache objCache = objSelectedNode.Tag as CharacterCache;
                objCache?.OnMyAfterSelect?.Invoke(sender, e);
                await UpdateCharacter(objCache, _objGenericToken).ConfigureAwait(false);
                await treCharacterList.DoThreadSafeAsync(x => x.ClearNodeBackground(x.SelectedNode), _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void treCharacterList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TreeNode objSelectedNode = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken).ConfigureAwait(false);
                if (objSelectedNode == null || objSelectedNode.Level <= 0)
                    return;
                switch (objSelectedNode.Tag)
                {
                    case null:
                        return;

                    case CharacterCache objCache:
                        CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                        try
                        {
                            objCache.OnMyDoubleClick(sender, e);
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }

                        break;
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
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

        private async void treCharacterList_OnDefaultDragDrop(object sender, DragEventArgs e)
        {
            // Do not allow the root element to be moved.
            if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 || treCharacterList.SelectedNode.Parent?.Tag?.ToString() == "Watch")
                return;

            if (!e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
                return;
            if (!(sender is TreeView treSenderView))
                return;
            Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
            if (nodDestinationNode?.Level > 0)
                nodDestinationNode = nodDestinationNode.Parent;
            string strDestinationNode = nodDestinationNode?.Tag?.ToString();
            try
            {
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
                                await GlobalSettings.FavoriteCharacters.RemoveAsync(
                                    objCache.FilePath, _objGenericToken).ConfigureAwait(false);
                                await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(
                                    0, objCache.FilePath, _objGenericToken).ConfigureAwait(false);
                                break;

                            case "Favorite":
                                await GlobalSettings.FavoriteCharacters.AddWithSortAsync(
                                    objCache.FilePath, token: _objGenericToken).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            IPlugin plugintag = null;
            while (nodDestinationNode?.Tag != null && plugintag == null)
            {
                if (nodDestinationNode.Tag is IPlugin temp)
                    plugintag = temp;
                nodDestinationNode = nodDestinationNode.Parent;
            }

            if (plugintag != null)
            {
                try
                {
                    await plugintag.DoCharacterList_DragDrop(sender, e, treCharacterList, _objGenericToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private void treCharacterList_OnDefaultItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private async void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                await ProcessMugshotSizeMode(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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

            try
            {
                TreeNode t = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken).ConfigureAwait(false);

                if (t?.Tag is CharacterCache objCache)
                {
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        Character objCharacter
                            = await Program.OpenCharacters.FirstOrDefaultAsync(
                                x => x.FileName == objCache.FileName, _objGenericToken).ConfigureAwait(false);
                        if (objCharacter == null)
                        {
                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = await Program.CreateAndShowProgressBarAsync(
                                                      objCache.FilePath, Character.NumLoadingSections, _objGenericToken)
                                                  .ConfigureAwait(false))
                            {
                                objCharacter = await Program.LoadCharacterAsync(
                                                                objCache.FilePath, frmLoadingBar: frmLoadingBar.MyForm,
                                                                token: _objGenericToken)
                                                            .ConfigureAwait(false);
                            }
                        }

                        if (!await Program.SwitchToOpenCharacter(objCharacter, _objGenericToken).ConfigureAwait(false))
                            await Program.OpenCharacter(objCharacter, token: _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void tsOpenForPrinting_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            try
            {
                TreeNode t = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken).ConfigureAwait(false);
                if (!(t?.Tag is CharacterCache objCache))
                    return;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Character objCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(
                            x => x.FileName == objCache.FileName, _objGenericToken).ConfigureAwait(false);
                    if (objCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(
                                                  objCache.FilePath, Character.NumLoadingSections, _objGenericToken)
                                              .ConfigureAwait(false))
                        {
                            objCharacter
                                = await Program.LoadCharacterAsync(objCache.FilePath,
                                                                   frmLoadingBar: frmLoadingBar.MyForm,
                                                                   token: _objGenericToken).ConfigureAwait(false);
                        }
                    }

                    if (!await Program.SwitchToOpenPrintCharacter(objCharacter, _objGenericToken).ConfigureAwait(false))
                        await Program.OpenCharacterForPrinting(objCharacter, token: _objGenericToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void tsOpenForExport_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            try
            {
                TreeNode t = await treCharacterList.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken).ConfigureAwait(false);
                if (!(t?.Tag is CharacterCache objCache))
                    return;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Character objCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(
                            x => x.FileName == objCache.FileName, _objGenericToken).ConfigureAwait(false);
                    if (objCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(
                                                  objCache.FilePath, Character.NumLoadingSections, _objGenericToken)
                                              .ConfigureAwait(false))
                        {
                            objCharacter
                                = await Program.LoadCharacterAsync(objCache.FilePath,
                                                                   frmLoadingBar: frmLoadingBar.MyForm,
                                                                   token: _objGenericToken).ConfigureAwait(false);
                        }
                    }

                    if (!await Program.SwitchToOpenExportCharacter(objCharacter, _objGenericToken).ConfigureAwait(false))
                        await Program.OpenCharacterForExport(objCharacter, token: _objGenericToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void tsToggleFav_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;

            TreeNode t = treCharacterList.SelectedNode;

            if (!(t?.Tag is CharacterCache objCache))
                return;
            try
            {
                switch (t.Parent.Tag.ToString())
                {
                    case "Favorite":
                        await GlobalSettings.FavoriteCharacters.RemoveAsync(objCache.FilePath, _objGenericToken).ConfigureAwait(false);
                        await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(
                            0, objCache.FilePath, _objGenericToken).ConfigureAwait(false);
                        break;

                    default:
                        await GlobalSettings.FavoriteCharacters.AddWithSortAsync(
                            objCache.FilePath, token: _objGenericToken).ConfigureAwait(false);
                        break;
                }

                treCharacterList.SelectedNode = t;
            }
            catch (OperationCanceledException)
            {
                // swallow this
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

        private async void tsCloseOpenCharacter_Click(object sender, EventArgs e)
        {
            if (treCharacterList.IsNullOrDisposed())
                return;
            try
            {
                TreeNode objSelectedNode = await treCharacterList
                                                 .DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken)
                                                 .ConfigureAwait(false);
                if (objSelectedNode?.Level <= 0)
                    return;
                string strFile = objSelectedNode?.Tag?.ToString();
                if (string.IsNullOrEmpty(strFile))
                    return;
                CursorWait objCursorWait
                    = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Character objOpenCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile, _objGenericToken)
                                       .ConfigureAwait(false);
                    if (objOpenCharacter != null)
                    {
                        Stack<Form> stkToClose = new Stack<Form>();
                        foreach (IHasCharacterObjects objOpenForm in Program.MainForm.OpenFormsWithCharacters)
                        {
                            _objGenericToken.ThrowIfCancellationRequested();
                            if (objOpenForm.CharacterObjects.Contains(objOpenCharacter)
                                && objOpenForm is Form frmOpenForm)
                            {
                                stkToClose.Push(frmOpenForm);
                            }
                        }
                        _objGenericToken.ThrowIfCancellationRequested();
                        while (stkToClose.Count > 0)
                            await stkToClose.Pop().DoThreadSafeAsync(x => x.Close(), _objGenericToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void TreCharacterList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    await treCharacterList.DoThreadSafeAsync(x => x.SelectedNode = e.Node, token: _objGenericToken)
                                          .ConfigureAwait(false);
                }

                if (e.Node.Tag != null)
                {
                    string strTag = e.Node.Tag.ToString();
                    if (!string.IsNullOrEmpty(strTag))
                    {
                        bool blnIncludeCloseOpenCharacter
                            = (strTag.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                               || strTag.EndsWith(
                                   ".chum5lz", StringComparison.OrdinalIgnoreCase))
                              && Program.MainForm.OpenFormsWithCharacters.Any(
                                  x => x.CharacterObjects.Any(y => y.FileName == strTag));
                        ContextMenuStrip objStrip =
                            await CreateContextMenuStripAsync(blnIncludeCloseOpenCharacter, _objGenericToken)
                                .ConfigureAwait(false);
                        await this.DoThreadSafeAsync(() => e.Node.ContextMenuStrip = objStrip, token: _objGenericToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        ContextMenuStrip objStrip = await CreateContextMenuStripAsync(false, _objGenericToken)
                            .ConfigureAwait(false);
                        await this.DoThreadSafeAsync(() => e.Node.ContextMenuStrip = objStrip, token: _objGenericToken)
                            .ConfigureAwait(false);
                    }
                }

                foreach (IPlugin plugin in await Program.PluginLoader.GetMyActivePluginsAsync(_objGenericToken)
                                                        .ConfigureAwait(false))
                {
                    await this.DoThreadSafeAsync(() => plugin.SetCharacterRosterNode(e.Node), token: _objGenericToken)
                              .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public ContextMenuStrip CreateContextMenuStrip(bool blnIncludeCloseOpenCharacter, CancellationToken token = default)
        {
            const int ToolStripWidth = 180;
            const int ToolStripHeight = 22;

            return this.DoThreadSafeFunc(x => CreateContextMenuStripCore(blnIncludeCloseOpenCharacter, ToolStripWidth, ToolStripHeight, x), token);
        }

        public Task<ContextMenuStrip> CreateContextMenuStripAsync(bool blnIncludeCloseOpenCharacter, CancellationToken token = default)
        {
            const int ToolStripWidth = 180;
            const int ToolStripHeight = 22;

            return this.DoThreadSafeFuncAsync(x => CreateContextMenuStripCore(blnIncludeCloseOpenCharacter, ToolStripWidth, ToolStripHeight, x), token);
        }

        private ContextMenuStrip CreateContextMenuStripCore(bool blnIncludeCloseOpenCharacter, int intToolStripWidth, int intToolStripHeight, CancellationToken token = default)
        {
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
                Name = "tsToggleFav",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_ToggleFavorite"
            };
            tsToggleFav.BatchSetImages(Properties.Resources.asterisk_orange_16,
                Properties.Resources.asterisk_orange_20, Properties.Resources.asterisk_orange_24,
                Properties.Resources.asterisk_orange_32, Properties.Resources.asterisk_orange_48,
                Properties.Resources.asterisk_orange_64);
            tsToggleFav.Click += tsToggleFav_Click;
            //
            // tsSort
            //
            DpiFriendlyToolStripMenuItem tsSort = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsSort",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Sort"
            };
            tsSort.BatchSetImages(Properties.Resources.page_refresh_16,
                Properties.Resources.page_refresh_20, Properties.Resources.page_refresh_24,
                Properties.Resources.page_refresh_32, Properties.Resources.page_refresh_48,
                Properties.Resources.page_refresh_64);
            tsSort.Click += tsSort_Click;
            //
            // tsOpen
            //
            DpiFriendlyToolStripMenuItem tsOpen = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsOpen",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_Open"
            };
            tsOpen.BatchSetImages(Properties.Resources.folder_page_16,
                Properties.Resources.folder_page_20, Properties.Resources.folder_page_24,
                Properties.Resources.folder_page_32, Properties.Resources.folder_page_48,
                Properties.Resources.folder_page_64);
            tsOpen.Click += tsOpen_Click;
            //
            // tsOpenForPrinting
            //
            DpiFriendlyToolStripMenuItem tsOpenForPrinting = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsOpenForPrinting",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_OpenForPrinting"
            };
            tsOpenForPrinting.BatchSetImages(Properties.Resources.folder_print_16,
                Properties.Resources.folder_print_20, Properties.Resources.folder_print_24,
                Properties.Resources.folder_print_32, Properties.Resources.folder_print_48,
                Properties.Resources.folder_print_64);
            tsOpenForPrinting.Click += tsOpenForPrinting_Click;
            //
            // tsOpenForExport
            //
            DpiFriendlyToolStripMenuItem tsOpenForExport = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsOpenForExport",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Main_OpenForExport"
            };
            tsOpenForExport.BatchSetImages(Properties.Resources.folder_script_go_16,
                Properties.Resources.folder_script_go_20, Properties.Resources.folder_script_go_24,
                Properties.Resources.folder_script_go_32, Properties.Resources.folder_script_go_48,
                Properties.Resources.folder_script_go_64);
            tsOpenForExport.Click += tsOpenForExport_Click;
            //
            // tsDelete
            //
            DpiFriendlyToolStripMenuItem tsDelete = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsDelete",
                Size = new Size(intToolStripWidth, intToolStripHeight),
                Tag = "Menu_Delete"
            };
            tsDelete.BatchSetImages(Properties.Resources.delete_16,
                Properties.Resources.delete_20, Properties.Resources.delete_24,
                Properties.Resources.delete_32, Properties.Resources.delete_48,
                Properties.Resources.delete_64);
            tsDelete.Click += tsDelete_Click;
            //
            // cmsRoster
            //
            ContextMenuStrip cmsRoster = new ContextMenuStrip(components)
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

            tsToggleFav.TranslateToolStripItemsRecursively(token: token);
            tsSort.TranslateToolStripItemsRecursively(token: token);
            tsOpen.TranslateToolStripItemsRecursively(token: token);
            tsOpenForPrinting.TranslateToolStripItemsRecursively(token: token);
            tsOpenForExport.TranslateToolStripItemsRecursively(token: token);
            tsDelete.TranslateToolStripItemsRecursively(token: token);

            if (blnIncludeCloseOpenCharacter)
            {
                //
                // tsCloseOpenCharacter
                //
                DpiFriendlyToolStripMenuItem tsCloseOpenCharacter = new DpiFriendlyToolStripMenuItem
                {
                    Name = "tsCloseOpenCharacter",
                    Size = new Size(intToolStripWidth, intToolStripHeight),
                    Tag = "Menu_Close"
                };
                tsCloseOpenCharacter.BatchSetImages(Properties.Resources.door_out_16,
                    Properties.Resources.door_out_20, Properties.Resources.door_out_24,
                    Properties.Resources.door_out_32, Properties.Resources.door_out_48,
                    Properties.Resources.door_out_64);
                tsCloseOpenCharacter.Click += tsCloseOpenCharacter_Click;
                cmsRoster.Items.Add(tsCloseOpenCharacter);
                tsCloseOpenCharacter.TranslateToolStripItemsRecursively(token: token);
            }

            cmsRoster.UpdateLightDarkMode(token);
            return cmsRoster;
        }

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }
    }
}
