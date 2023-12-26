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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;
using Application = System.Windows.Forms.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using Path = System.IO.Path;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace Chummer
{
    public sealed partial class ChummerMainForm : Form
    {
        private bool _blnAbleToReceiveData;
        private int _intFormClosing;
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private ChummerUpdater _frmUpdate;
        private DiceRoller _frmDiceRoller;
        private ThreadSafeObservableCollection<CharacterShared> _lstOpenCharacterEditorForms
            = new ThreadSafeObservableCollection<CharacterShared>();
        private ThreadSafeObservableCollection<CharacterSheetViewer> _lstOpenCharacterSheetViewers
            = new ThreadSafeObservableCollection<CharacterSheetViewer>();
        private ThreadSafeObservableCollection<ExportCharacter> _lstOpenCharacterExportForms
            = new ThreadSafeObservableCollection<ExportCharacter>();
        private ConcurrentStringHashSet _setCharactersToOpen;
        private readonly Timer _tmrCharactersToOpenCheck = new Timer();
        private readonly string _strCurrentVersion;
        private Chummy _mascotChummy;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;
        private readonly DebuggableSemaphoreSlim _objFormOpeningSemaphore = new DebuggableSemaphoreSlim();

        public string MainTitle
        {
            get
            {
                try
                {
                    string strSpace = LanguageManager.GetString("String_Space", token: _objGenericToken);
                    string strTitle = Application.ProductName + strSpace + '-' + strSpace
                                      + LanguageManager.GetString("String_Version", token: _objGenericToken) + strSpace
                                      + _strCurrentVersion;
#if DEBUG
                    strTitle += " DEBUG BUILD";
#endif
                    return strTitle;
                }
                catch (OperationCanceledException)
                {
                    return string.Empty;
                }
            }
        }

#region Control Events

        public ChummerMainForm(bool blnIsUnitTest = false, bool blnIsUnitTestForUI = false)
        {
            _objGenericToken = _objGenericCancellationTokenSource.Token;
#if !DEBUG
            Disposed += (sender, args) => _objVersionUpdaterCancellationTokenSource?.Dispose();
#endif
            Utils.IsUnitTest = blnIsUnitTest;
            Utils.IsUnitTestForUI = blnIsUnitTestForUI;
            InitializeComponent();
#if DEBUG
            DpiFriendlyToolStripMenuItem mnuForceCrash = new DpiFriendlyToolStripMenuItem
            {
                Name = "mnuForceCrash",
                Text = "&Force Crash"
            };
            mnuForceCrash.BatchSetImages(Properties.Resources.error_16, Properties.Resources.error_20,
                Properties.Resources.error_24, Properties.Resources.error_32, Properties.Resources.error_48,
                Properties.Resources.error_32);
            mnuForceCrash.Click += mnuForceCrash_Click;
            toolsMenu.DropDownItems.Add(mnuForceCrash);
#endif
            tabForms.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _strCurrentVersion = Utils.CurrentChummerVersion.ToString(3);

            Disposed += (sender, args) =>
            {
                _tmrCharactersToOpenCheck.Dispose();
                _objGenericCancellationTokenSource.Dispose();
                _objFormOpeningSemaphore.Dispose();
                DisposeOpenForms();
            };

            _lstOpenCharacterEditorForms.BeforeClearCollectionChangedAsync += OpenCharacterEditorFormsOnBeforeClearCollectionChanged;
            _lstOpenCharacterEditorForms.CollectionChangedAsync += OpenCharacterEditorFormsOnCollectionChanged;
            _lstOpenCharacterSheetViewers.BeforeClearCollectionChangedAsync += OpenCharacterSheetViewersOnBeforeClearCollectionChanged;
            _lstOpenCharacterSheetViewers.CollectionChangedAsync += OpenCharacterSheetViewersOnCollectionChanged;
            _lstOpenCharacterExportForms.BeforeClearCollectionChangedAsync += OpenCharacterExportFormsOnBeforeClearCollectionChanged;
            _lstOpenCharacterExportForms.CollectionChangedAsync += OpenCharacterExportFormsOnCollectionChanged;
            _tmrCharactersToOpenCheck.Interval = 1000;
            _tmrCharactersToOpenCheck.Tick += CharactersToOpenCheckOnTick;

            //lets write that in separate lines to see where the exception is thrown
            if (!GlobalSettings.HideMasterIndex || blnIsUnitTest)
            {
                MasterIndex frmMasterIndex = new MasterIndex
                {
                    MdiParent = this
                };
                if (Interlocked.CompareExchange(ref _frmMasterIndex, frmMasterIndex, null) == null)
                    frmMasterIndex.FormClosed += (sender, args) => Interlocked.CompareExchange(ref _frmMasterIndex, null, frmMasterIndex);
                else
                    frmMasterIndex.Close();
            }
            if (!GlobalSettings.HideCharacterRoster || blnIsUnitTest)
            {
                CharacterRoster frmCharacterRoster = new CharacterRoster
                {
                    MdiParent = this
                };
                if (Interlocked.CompareExchange(ref _frmCharacterRoster, frmCharacterRoster, null) == null)
                    frmCharacterRoster.FormClosed += (sender, args) => Interlocked.CompareExchange(ref _frmCharacterRoster, null, frmCharacterRoster);
                else
                    frmCharacterRoster.Close();
            }
        }

        private async void CharactersToOpenCheckOnTick(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest || _intFormClosing > 0)
                return;
            try
            {
                await ProcessQueuedCharactersToOpen(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task OpenCharacterExportFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Utils.IsUnitTest || _intFormClosing > 0)
                return;
            try
            {
                foreach (ExportCharacter objOldForm in e.OldItems)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (Character objCharacter in objOldForm.CharacterObjects)
                    {
                        token.ThrowIfCancellationRequested();
                        if (objCharacter == null)
                            continue;
                        if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                        {
                            if (await Program.OpenCharacters.AllAsync(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                && Program.MainForm.OpenFormsWithCharacters.All(
                                    x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                        }
                        else
                            await objCharacter.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task OpenCharacterExportFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intFormClosing > 0)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ExportCharacter objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<ExportCharacter> lstToProcess = OpenCharacterExportForms;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(objNewForm, _objGenericToken)
                                                      .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    try
                    {
                        foreach (ExportCharacter objOldForm in e.OldItems)
                        {
                            token.ThrowIfCancellationRequested();
                            foreach (Character objCharacter in objOldForm.CharacterObjects)
                            {
                                token.ThrowIfCancellationRequested();
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                        && Program.MainForm.OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                                }
                                else
                                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        try
                        {
                            foreach (ExportCharacter objOldForm in e.OldItems)
                            {
                                token.ThrowIfCancellationRequested();
                                if (e.NewItems.Contains(objOldForm))
                                    continue;
                                foreach (Character objCharacter in objOldForm.CharacterObjects)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (objCharacter == null)
                                        continue;
                                    if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                                    {
                                        if (await Program.OpenCharacters.AllAsync(
                                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                            && Program.MainForm.OpenFormsWithCharacters.All(
                                                x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                            await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                                    }
                                    else
                                        await objCharacter.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }

                    foreach (ExportCharacter objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        if (e.OldItems.Contains(objNewForm))
                            continue;

                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<ExportCharacter> lstToProcess = OpenCharacterExportForms;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(objNewForm, _objGenericToken)
                                                      .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        private async Task OpenCharacterSheetViewersOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Utils.IsUnitTest || _intFormClosing > 0)
                return;
            try
            {
                foreach (CharacterSheetViewer objOldForm in e.OldItems)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (Character objCharacter in objOldForm.CharacterObjects)
                    {
                        token.ThrowIfCancellationRequested();
                        if (objCharacter == null)
                            continue;
                        if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                        {
                            if (await Program.OpenCharacters.AllAsync(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                && OpenFormsWithCharacters.All(
                                    x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                        }
                        else
                            await objCharacter.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task OpenCharacterSheetViewersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intFormClosing > 0)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterSheetViewer objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess
                                = OpenCharacterSheetViewers;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(objNewForm, _objGenericToken)
                                                      .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    try
                    {
                        foreach (CharacterSheetViewer objOldForm in e.OldItems)
                        {
                            token.ThrowIfCancellationRequested();
                            foreach (Character objCharacter in objOldForm.CharacterObjects)
                            {
                                token.ThrowIfCancellationRequested();
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                        && OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                                }
                                else
                                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        foreach (CharacterSheetViewer objOldForm in e.OldItems)
                        {
                            token.ThrowIfCancellationRequested();
                            if (e.NewItems.Contains(objOldForm))
                                continue;
                            foreach (Character objCharacter in objOldForm.CharacterObjects)
                            {
                                token.ThrowIfCancellationRequested();
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                        && Program.MainForm.OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                                }
                                else
                                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }

                    foreach (CharacterSheetViewer objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        if (e.OldItems.Contains(objNewForm))
                            continue;
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess
                                = OpenCharacterSheetViewers;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(objNewForm, _objGenericToken)
                                                      .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        private int _intSkipReopenUntilAllClear;
        private ConcurrentBag<Character> _lstCharactersToReopen = new ConcurrentBag<Character>();

        private async Task OpenCharacterEditorFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Utils.IsUnitTest || _intFormClosing > 0)
                return;
            Interlocked.Increment(ref _intSkipReopenUntilAllClear);
            try
            {
                foreach (CharacterShared objOldForm in e.OldItems)
                {
                    token.ThrowIfCancellationRequested();
                    if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                    {
                        _lstCharactersToReopen.Add(objOldCreateForm.CharacterObject);
                        continue;
                    }

                    Character objCharacter = objOldForm.CharacterObject;
                    if (objCharacter == null)
                        continue;
                    if (await Program.OpenCharacters.ContainsAsync(objCharacter, token)
                                     .ConfigureAwait(false))
                    {
                        if (await Program.OpenCharacters
                                         .AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter),
                                                   token: token).ConfigureAwait(false)
                            && Program.MainForm.OpenFormsWithCharacters.All(
                                x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                            await Program.OpenCharacters.RemoveAsync(objCharacter, token)
                                         .ConfigureAwait(false);
                    }
                    else
                        await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                Interlocked.Decrement(ref _intSkipReopenUntilAllClear);
                //swallow this
            }
            catch
            {
                Interlocked.Decrement(ref _intSkipReopenUntilAllClear);
                throw;
            }
        }

        private async Task OpenCharacterEditorFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intFormClosing > 0)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterShared objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<CharacterShared> lstToProcess = OpenCharacterEditorForms;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(
                                        objNewForm, _objGenericToken).ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    try
                    {
                        foreach (CharacterShared objOldForm in e.OldItems)
                        {
                            token.ThrowIfCancellationRequested();
                            if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                            {
                                _lstCharactersToReopen.Add(objOldCreateForm.CharacterObject);
                                continue;
                            }
                            Character objCharacter = objOldForm.CharacterObject;
                            if (objCharacter == null)
                                continue;
                            if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                            {
                                if (await Program.OpenCharacters.AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                    && Program.MainForm.OpenFormsWithCharacters.All(x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                            }
                            else
                                await objCharacter.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        try
                        {
                            foreach (CharacterShared objOldForm in e.OldItems)
                            {
                                token.ThrowIfCancellationRequested();
                                if (e.NewItems.Contains(objOldForm))
                                    continue;
                                if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                                {
                                    _lstCharactersToReopen.Add(objOldCreateForm.CharacterObject);
                                    continue;
                                }
                                Character objCharacter = objOldForm.CharacterObject;
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter, token).ConfigureAwait(false))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter), token: token).ConfigureAwait(false)
                                        && Program.MainForm.OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token).ConfigureAwait(false);
                                }
                                else
                                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }

                    foreach (CharacterShared objNewForm in e.NewItems)
                    {
                        token.ThrowIfCancellationRequested();
                        if (e.OldItems.Contains(objNewForm))
                            continue;
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args)
                        {
                            ThreadSafeObservableCollection<CharacterShared> lstToProcess = OpenCharacterEditorForms;
                            if (lstToProcess != null)
                            {
                                try
                                {
                                    await lstToProcess.RemoveAsync(objNewForm, _objGenericToken)
                                                      .ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }
                        }

                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        //Moved most of the initialization out of the constructor to allow the Mainform to be generated fast
        //in case of a commandline argument not asking for the mainform to be shown.
        private async void ChummerMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait
                    = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    using (CustomActivity opFrmChummerMain = Timekeeper.StartSyncron(
                               "frmChummerMain_Load", null, CustomActivity.OperationType.DependencyOperation,
                               _strCurrentVersion))
                    {
                        try
                        {
                            opFrmChummerMain.MyDependencyTelemetry.Type = "loadfrmChummerMain";
                            opFrmChummerMain.MyDependencyTelemetry.Target = _strCurrentVersion;

                            if (MyStartupPvt != null)
                            {
                                MyStartupPvt.Duration = DateTimeOffset.UtcNow - MyStartupPvt.Timestamp;
                                opFrmChummerMain.MyTelemetryClient.TrackPageView(MyStartupPvt);
                            }

                            NativeMethods.ChangeFilterStruct changeFilter = default;
                            changeFilter.size = (uint) Marshal.SizeOf(changeFilter);
                            changeFilter.info = 0;
                            if (NativeMethods.ChangeWindowMessageFilterEx(
                                    await this.DoThreadSafeFuncAsync(x => x.Handle, token: _objGenericToken)
                                              .ConfigureAwait(false),
                                    NativeMethods.WM_COPYDATA,
                                    NativeMethods.ChangeWindowMessageFilterExAction
                                                 .Allow, ref changeFilter))
                                _blnAbleToReceiveData = true;
                            else
                            {
                                int intErrorCode = Marshal.GetLastWin32Error();
                                Utils.BreakIfDebug();
                                Log.Error(
                                    "The error " + intErrorCode + " occurred while attempting to unblock WM_COPYDATA.");
                            }

                            await this.DoThreadSafeAsync(x => x.Text = MainTitle, token: _objGenericToken)
                                      .ConfigureAwait(false);
                            dlgOpenFile.Filter
                                = await LanguageManager.GetStringAsync("DialogFilter_Chummer", token: _objGenericToken)
                                                       .ConfigureAwait(false)
                                  + '|' +
                                  await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: _objGenericToken)
                                                       .ConfigureAwait(false)
                                  + '|' +
                                  await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: _objGenericToken)
                                                       .ConfigureAwait(false)
                                  + '|' +
                                  await LanguageManager.GetStringAsync("DialogFilter_All", token: _objGenericToken)
                                                       .ConfigureAwait(false);

                            //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;

#if !DEBUG
                            // If Automatic Updates are enabled, check for updates immediately.
                            StartAutoUpdateChecker(_objGenericToken);
#endif

                            GlobalSettings.MruChanged += PopulateMruToolstripMenu;

                            // Populate the MRU list.
                            await DoPopulateMruToolstripMenu(token: _objGenericToken).ConfigureAwait(false);

                            Program.MainForm = this;

                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = await Program.CreateAndShowProgressBarAsync(
                                                      Text,
                                                      GlobalSettings.AllowEasterEggs ? 4 : 3, _objGenericToken)
                                                  .ConfigureAwait(false))
                            {
                                await frmLoadingBar.MyForm.PerformStepAsync(
                                    await LanguageManager.GetStringAsync("String_UI", token: _objGenericToken)
                                                         .ConfigureAwait(false),
                                    token: _objGenericToken).ConfigureAwait(false);

                                Program.OpenCharacters.BeforeClearCollectionChanged += OpenCharactersOnBeforeClearCollectionChanged;
                                Program.OpenCharacters.CollectionChanged += OpenCharactersOnCollectionChanged;

                                // Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
                                bool blnShowTest = false;
                                if (!Utils.IsUnitTest)
                                {
                                    string[] strArgs = Environment.GetCommandLineArgs();
                                    using (ProcessCommandLineArguments(strArgs, out blnShowTest,
                                               out HashSet<string> setFilesToLoad,
                                               opFrmChummerMain))
                                    {
                                        if (Directory.Exists(Utils.GetAutosavesFolderPath))
                                        {
                                            // Always process newest autosave if all MRUs are empty
                                            bool blnAnyAutosaveInMru
                                                = GlobalSettings.MostRecentlyUsedCharacters.Count == 0 &&
                                                  GlobalSettings.FavoriteCharacters.Count == 0;
                                            FileInfo objMostRecentAutosave = null;
                                            List<string> lstOldAutosaves = new List<string>(10);
                                            DateTime objOldAutosaveTimeThreshold =
                                                DateTime.UtcNow.Subtract(TimeSpan.FromDays(90));
                                            foreach (string strAutosave in Directory
                                                                           .EnumerateFiles(
                                                                               Utils.GetAutosavesFolderPath,
                                                                               "*.chum5",
                                                                               SearchOption.AllDirectories)
                                                                           .Concat(Directory.EnumerateFiles(
                                                                               Utils.GetAutosavesFolderPath,
                                                                               "*.chum5lz",
                                                                               SearchOption.AllDirectories)))
                                            {
                                                FileInfo objAutosave;
                                                try
                                                {
                                                    objAutosave = new FileInfo(strAutosave);
                                                }
                                                catch (SecurityException)
                                                {
                                                    continue;
                                                }
                                                catch (UnauthorizedAccessException)
                                                {
                                                    continue;
                                                }

                                                if (objMostRecentAutosave == null || objAutosave.LastWriteTimeUtc >
                                                    objMostRecentAutosave.LastWriteTimeUtc)
                                                    objMostRecentAutosave = objAutosave;
                                                string strAutosaveName
                                                    = Path.GetFileNameWithoutExtension(objAutosave.Name);
                                                if (await GlobalSettings.MostRecentlyUsedCharacters.AnyAsync(x =>
                                                        Path.GetFileNameWithoutExtension(x) == strAutosaveName, _objGenericToken).ConfigureAwait(false)
                                                    ||
                                                    await GlobalSettings.FavoriteCharacters.AnyAsync(x =>
                                                        Path.GetFileNameWithoutExtension(x) == strAutosaveName, _objGenericToken).ConfigureAwait(false))
                                                    blnAnyAutosaveInMru = true;
                                                else if (objAutosave != objMostRecentAutosave &&
                                                         objAutosave.LastWriteTimeUtc < objOldAutosaveTimeThreshold
                                                         &&
                                                         !setFilesToLoad.Contains(strAutosave))
                                                    lstOldAutosaves.Add(strAutosave);
                                            }

                                            if (objMostRecentAutosave != null)
                                            {
                                                // Might have had a crash for an unsaved character, so prompt if we want to load them
                                                if (blnAnyAutosaveInMru &&
                                                    !setFilesToLoad.Contains(objMostRecentAutosave.FullName))
                                                {
                                                    string strAutosaveName
                                                        = Path.GetFileNameWithoutExtension(
                                                            objMostRecentAutosave.Name);
                                                    if (await GlobalSettings.MostRecentlyUsedCharacters.AllAsync(
                                                            x => Path.GetFileNameWithoutExtension(x)
                                                                 != strAutosaveName, _objGenericToken).ConfigureAwait(false) &&
                                                        await GlobalSettings.FavoriteCharacters.AllAsync(
                                                            x => Path.GetFileNameWithoutExtension(x)
                                                                 != strAutosaveName, _objGenericToken).ConfigureAwait(false))
                                                    {
                                                        if (Program.ShowScrollableMessageBox(
                                                                string.Format(GlobalSettings.CultureInfo,
                                                                              await LanguageManager.GetStringAsync(
                                                                                      "Message_PossibleCrashAutosaveFound",
                                                                                      token: _objGenericToken)
                                                                                  .ConfigureAwait(false),
                                                                              objMostRecentAutosave.Name,
                                                                              objMostRecentAutosave.LastWriteTimeUtc
                                                                                  .ToLocalTime()),
                                                                await LanguageManager.GetStringAsync(
                                                                    "MessageTitle_AutosaveFound",
                                                                    token: _objGenericToken).ConfigureAwait(false),
                                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                                            == DialogResult.Yes)
                                                            setFilesToLoad.Add(objMostRecentAutosave.FullName);
                                                        else
                                                            lstOldAutosaves.Add(objMostRecentAutosave.FullName);
                                                    }
                                                    else if (objMostRecentAutosave.LastWriteTimeUtc
                                                             < objOldAutosaveTimeThreshold)
                                                        lstOldAutosaves.Add(objMostRecentAutosave.FullName);
                                                }
                                                else if (objMostRecentAutosave.LastWriteTimeUtc
                                                         < objOldAutosaveTimeThreshold)
                                                    lstOldAutosaves.Add(objMostRecentAutosave.FullName);
                                            }

                                            // Delete all old autosaves
                                            List<Task> lstTasks = new List<Task>(lstOldAutosaves.Count);
                                            foreach (string strOldAutosave in lstOldAutosaves)
                                            {
                                                lstTasks.Add(
                                                    FileExtensions.SafeDeleteAsync(
                                                        strOldAutosave, token: _objGenericToken));
                                            }

                                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                        }

                                        if (setFilesToLoad.Count > 0)
                                        {
                                            ConcurrentStringHashSet setNewCharactersToOpen
                                                = new ConcurrentStringHashSet();
                                            ConcurrentStringHashSet setCharactersToOpen
                                                = Interlocked.CompareExchange(
                                                    ref _setCharactersToOpen, setNewCharactersToOpen, null);
                                            if (setCharactersToOpen != null)
                                                setNewCharactersToOpen = setCharactersToOpen;
                                            foreach (string strFile in setFilesToLoad)
                                                setNewCharactersToOpen.TryAdd(strFile);
                                            _tmrCharactersToOpenCheck.Start();
                                        }
                                    }
                                }

                                await frmLoadingBar.MyForm.PerformStepAsync(
                                    await LanguageManager.GetStringAsync(
                                        "Title_MasterIndex", token: _objGenericToken).ConfigureAwait(false),
                                    token: _objGenericToken).ConfigureAwait(false);

                                MasterIndex frmMasterIndex = MasterIndex;
                                if (frmMasterIndex != null)
                                {
                                    await frmMasterIndex.DoThreadSafeAsync(x => x.Show(), token: _objGenericToken)
                                                        .ConfigureAwait(false);
                                }

                                await frmLoadingBar.MyForm.PerformStepAsync(
                                    await LanguageManager.GetStringAsync(
                                        "String_CharacterRoster", token: _objGenericToken).ConfigureAwait(false),
                                    token: _objGenericToken).ConfigureAwait(false);

                                CharacterRoster frmCharacterRoster = CharacterRoster;
                                if (frmCharacterRoster != null)
                                {
                                    await frmCharacterRoster.DoThreadSafeAsync(x => x.Show(), token: _objGenericToken)
                                                            .ConfigureAwait(false);
                                }

                                if (GlobalSettings.AllowEasterEggs)
                                {
                                    await frmLoadingBar.MyForm.PerformStepAsync(
                                        await LanguageManager.GetStringAsync(
                                            "String_Chummy", token: _objGenericToken).ConfigureAwait(false),
                                        token: _objGenericToken).ConfigureAwait(false);
                                    _mascotChummy = await this.DoThreadSafeFuncAsync(x =>
                                    {
                                        Chummy objReturn = new Chummy(null);
                                        x.Disposed += (o, args) =>
                                        {
                                            if (Interlocked.CompareExchange(ref _mascotChummy, null, objReturn)
                                                == objReturn)
                                                objReturn.Dispose();
                                        };
                                        return objReturn;
                                    }, token: _objGenericToken).ConfigureAwait(false);
                                    await _mascotChummy.DoThreadSafeAsync(
                                        x => x.Show(), token: _objGenericToken).ConfigureAwait(false);
                                }

                                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                                frmMasterIndex = MasterIndex;
                                frmCharacterRoster = CharacterRoster;
                                if (frmMasterIndex != null)
                                {
                                    await frmMasterIndex.DoThreadSafeAsync(
                                        x =>
                                        {
                                            if (x.WindowState == FormWindowState.Normal)
                                                x.WindowState = FormWindowState.Maximized;
                                        },
                                        token: _objGenericToken).ConfigureAwait(false);
                                }

                                if (frmCharacterRoster != null)
                                {
                                    await frmCharacterRoster.DoThreadSafeAsync(
                                        x =>
                                        {
                                            if (x.WindowState == FormWindowState.Normal)
                                                x.WindowState = FormWindowState.Maximized;
                                        },
                                        token: _objGenericToken).ConfigureAwait(false);
                                }

                                if (blnShowTest)
                                {
                                    TestDataEntries frmTestData
                                        = await this.DoThreadSafeFuncAsync(
                                                        () => new TestDataEntries(), token: _objGenericToken)
                                                    .ConfigureAwait(false);
                                    await frmTestData.DoThreadSafeAsync(x => x.Show(), token: _objGenericToken)
                                                     .ConfigureAwait(false);
                                }

                                await Program.PluginLoader.CallPlugins(
                                    toolsMenu, opFrmChummerMain, _objGenericToken).ConfigureAwait(false);

                                // Set the Tag for each ToolStrip item so it can be translated.
                                await menuStrip.DoThreadSafeAsync(x =>
                                {
                                    foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                                    {
                                        tssItem.UpdateLightDarkMode(token: _objGenericToken);
                                    }
                                }, token: _objGenericToken).ConfigureAwait(false);
                                await mnuProcessFile.DoThreadSafeAsync(x =>
                                {
                                    foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                                    {
                                        tssItem.UpdateLightDarkMode(token: _objGenericToken);
                                    }
                                }, token: _objGenericToken).ConfigureAwait(false);
                                List<Tuple<ToolStripItem, string>> lstToTranslate
                                    = new List<Tuple<ToolStripItem, string>>();
                                foreach (ToolStripItem tssItem in await menuStrip
                                                                        .DoThreadSafeFuncAsync(
                                                                            (x, y) => x.Items, _objGenericToken)
                                                                        .ConfigureAwait(false))
                                    lstToTranslate.AddRange(
                                        await menuStrip.TranslateToolStripItemsRecursivelyPrepAsync(
                                            tssItem, token: _objGenericToken).ConfigureAwait(false));
                                foreach ((ToolStripItem objControl, string strTag) in lstToTranslate)
                                {
                                    string strText = await LanguageManager.GetStringAsync(strTag, token: _objGenericToken).ConfigureAwait(false);
                                    await menuStrip.DoThreadSafeAsync(() => objControl.Text = strText, token: _objGenericToken).ConfigureAwait(false);
                                }
                                lstToTranslate.Clear();
                                foreach (ToolStripItem tssItem in await mnuProcessFile
                                                                        .DoThreadSafeFuncAsync(
                                                                            (x, y) => x.Items, _objGenericToken)
                                                                        .ConfigureAwait(false))
                                    lstToTranslate.AddRange(
                                        await mnuProcessFile.TranslateToolStripItemsRecursivelyPrepAsync(
                                            tssItem, token: _objGenericToken).ConfigureAwait(false));
                                foreach ((ToolStripItem objControl, string strTag) in lstToTranslate)
                                {
                                    string strText = await LanguageManager.GetStringAsync(strTag, token: _objGenericToken).ConfigureAwait(false);
                                    await mnuProcessFile.DoThreadSafeAsync(() => objControl.Text = strText, token: _objGenericToken).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (opFrmChummerMain != null)
                            {
                                opFrmChummerMain.SetSuccess(false);
                                opFrmChummerMain.MyTelemetryClient.TrackException(ex);
                            }

                            Log.Error(ex);
                            throw;
                        }

                        //sometimes the Configuration gets messed up - make sure it is valid!
                        try
                        {
                            Size _ = Properties.Settings.Default.Size;
                        }
                        catch (ArgumentException ex)
                        {
                            //the config is invalid - reset it!
                            Properties.Settings.Default.Reset();
                            Properties.Settings.Default.Save();
                            Log.Warn(
                                "Configuartion Settings were invalid and had to be reset. Exception: " + ex.Message);
                        }
                        catch (System.Configuration.ConfigurationErrorsException ex)
                        {
                            //the config is invalid - reset it!
                            Properties.Settings.Default.Reset();
                            Properties.Settings.Default.Save();
                            Log.Warn(
                                "Configuartion Settings were invalid and had to be reset. Exception: " + ex.Message);
                        }

                        if (Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0
                                                                        || !IsVisibleOnAnyScreen())
                        {
                            int intDefaultWidth = 1280;
                            int intDefaultHeight = 720;
                            using (Graphics g = CreateGraphics())
                            {
                                intDefaultWidth = (int)(intDefaultWidth * g.DpiX / 96.0f);
                                intDefaultHeight = (int)(intDefaultHeight * g.DpiY / 96.0f);
                            }

                            await this.DoThreadSafeAsync(x =>
                            {
                                x.Size = new Size(intDefaultWidth, intDefaultHeight);
                                x.StartPosition = FormStartPosition.CenterScreen;
                            }, token: _objGenericToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await this.DoThreadSafeAsync(x =>
                            {
                                if (!Utils.IsUnitTest)
                                {
                                    x.WindowState = Properties.Settings.Default.WindowState;
                                    if (x.WindowState == FormWindowState.Minimized)
                                        x.WindowState = FormWindowState.Normal;
                                }

                                x.Location = Properties.Settings.Default.Location;
                                x.Size = Properties.Settings.Default.Size;
                            }, token: _objGenericToken).ConfigureAwait(false);
                        }

                        if (!Utils.IsUnitTest && GlobalSettings.StartupFullscreen)
                            await this.DoThreadSafeAsync(x => x.WindowState = FormWindowState.Maximized,
                                                         token: _objGenericToken).ConfigureAwait(false);
                    }

                    if (Utils.IsUnitTestForUI)
                    {
                        while (CharacterRoster?.IsFinishedLoading == false || MasterIndex?.IsFinishedLoading == false)
                            await Utils.SafeSleepAsync(_objGenericToken).ConfigureAwait(false);
                    }

                    IsFinishedLoading = true;
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

        [CLSCompliant(false)]
        public PageViewTelemetry MyStartupPvt { get; set; }

        private void OpenCharactersOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Character objCharacter in e.OldItems)
            {
                if (objCharacter?.IsDisposed != false)
                    continue;
                try
                {
                    objCharacter.PropertyChangedAsync -= UpdateCharacterTabTitle;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
            }
        }

        private void OpenCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (Character objCharacter in Program.OpenCharacters)
                        {
                            objCharacter.PropertyChangedAsync += UpdateCharacterTabTitle;
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Character objCharacter in e.NewItems)
                        {
                            objCharacter.PropertyChangedAsync += UpdateCharacterTabTitle;
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Character objCharacter in e.OldItems)
                        {
                            if (objCharacter?.IsDisposed != false)
                                continue;
                            try
                            {
                                objCharacter.PropertyChangedAsync -= UpdateCharacterTabTitle;
                            }
                            catch (ObjectDisposedException)
                            {
                                //swallow this
                            }
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Character objCharacter in e.OldItems)
                        {
                            if (objCharacter?.IsDisposed != false)
                                continue;
                            try
                            {
                                objCharacter.PropertyChangedAsync -= UpdateCharacterTabTitle;
                            }
                            catch (ObjectDisposedException)
                            {
                                //swallow this
                            }
                        }

                        foreach (Character objCharacter in e.NewItems)
                        {
                            objCharacter.PropertyChangedAsync += UpdateCharacterTabTitle;
                        }

                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public CharacterRoster CharacterRoster
        {
            get
            {
                CharacterRoster frmReturn = _frmCharacterRoster;
                if (frmReturn == null)
                    return null;

                if (frmReturn.Disposing || frmReturn.IsDisposed)
                {
                    Interlocked.CompareExchange(ref _frmCharacterRoster, null, frmReturn);
                    return null;
                }

                return frmReturn;
            }
        }

        public MasterIndex MasterIndex
        {
            get
            {
                MasterIndex frmReturn = _frmMasterIndex;
                if (frmReturn == null)
                    return null;

                if (frmReturn.Disposing || frmReturn.IsDisposed)
                {
                    Interlocked.CompareExchange(ref _frmMasterIndex, null, frmReturn);
                    return null;
                }

                return frmReturn;
            }
        }

        private MasterIndex _frmMasterIndex;
        private CharacterRoster _frmCharacterRoster;

#if !DEBUG
        private Uri UpdateLocation { get; } = new Uri(GlobalSettings.PreferNightlyBuilds
            ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
            : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest");

        private Task _tskVersionUpdate;

        private CancellationTokenSource _objVersionUpdaterCancellationTokenSource;

        private async Task DoCacheGitVersion(CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                token.ThrowIfCancellationRequested();
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                System.Net.HttpWebRequest request;
                try
                {
                    System.Net.WebRequest objTemp = System.Net.WebRequest.Create(UpdateLocation);
                    request = objTemp as System.Net.HttpWebRequest;
                }
                catch (SecurityException ex)
                {
                    Utils.CachedGitVersion = null;
                    Log.Error(ex);
                    return;
                }

                if (request == null)
                {
                    Utils.CachedGitVersion = null;
                    return;
                }

                token.ThrowIfCancellationRequested();

                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                request.Accept = "application/json";

                try
                {
                    // Get the response.
                    using (System.Net.HttpWebResponse response
                           = await request.GetResponseAsync() as System.Net.HttpWebResponse)
                    {
                        if (response == null)
                        {
                            Utils.CachedGitVersion = null;
                            return;
                        }

                        token.ThrowIfCancellationRequested();

                        // Get the stream containing content returned by the server.
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            if (dataStream == null)
                            {
                                Utils.CachedGitVersion = null;
                                return;
                            }

                            token.ThrowIfCancellationRequested();

                            // Open the stream using a StreamReader for easy access.
                            using (StreamReader objReader = new StreamReader(dataStream, System.Text.Encoding.UTF8, true))
                            {
                                token.ThrowIfCancellationRequested();

                                string strVersionLine = null;
                                // Read the content.
                                for (string strLine = await objReader.ReadLineAsync().ConfigureAwait(false);
                                     strLine != null;
                                     strLine = await objReader.ReadLineAsync().ConfigureAwait(false))
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (strLine.Contains("tag_name"))
                                    {
                                        strVersionLine = strLine.Substring(strLine.IndexOf(':') + 1).TrimEnd(',');
                                        break;
                                    }
                                }

                                token.ThrowIfCancellationRequested();

                                Version verLatestVersion = null;
                                if (!string.IsNullOrEmpty(strVersionLine))
                                {
                                    string strVersion = strVersionLine.Substring(strVersionLine.IndexOf(':') + 1);
                                    int intPos = strVersion.IndexOf('}');
                                    if (intPos != -1)
                                        strVersion = strVersion.Substring(0, intPos);
                                    strVersion = strVersion.FastEscape('\"');

                                    // Adds zeroes if minor and/or build version are missing
                                    if (strVersion.Count(x => x == '.') < 2)
                                    {
                                        strVersion += ".0";
                                        if (strVersion.Count(x => x == '.') < 2)
                                        {
                                            strVersion += ".0";
                                        }
                                    }

                                    token.ThrowIfCancellationRequested();

                                    if (!Version.TryParse(strVersion.TrimStartOnce("Nightly-v"), out verLatestVersion))
                                        verLatestVersion = null;

                                    token.ThrowIfCancellationRequested();
                                }

                                Utils.CachedGitVersion = verLatestVersion;
                            }
                        }
                    }
                }
                catch (System.Net.WebException ex)
                {
                    Utils.CachedGitVersion = null;
                    Log.Error(ex);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private void StartAutoUpdateChecker(CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CancellationTokenSource objNewSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewSource.Token;
                CancellationTokenSource objTemp
                    = Interlocked.Exchange(ref _objVersionUpdaterCancellationTokenSource, objNewSource);
                if (objTemp?.IsCancellationRequested == false)
                {
                    try
                    {
                        objTemp.Cancel(false);
                    }
                    finally
                    {
                        objTemp.Dispose();
                    }
                }
                
                try
                {
                    Task tskOld = Interlocked.Exchange(ref _tskVersionUpdate, null);
                    if (tskOld != null)
                    {
                        while (!tskOld.IsCompleted)
                            // ReSharper disable once MethodSupportsCancellation
                            Utils.SafeSleep(token);
                        if (tskOld.Exception != null)
                            throw tskOld.Exception;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (token.IsCancellationRequested)
                    {
                        Interlocked.CompareExchange(ref _objVersionUpdaterCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }

                    // swallow in all other cases
                    return;
                }
                catch
                {
                    Interlocked.CompareExchange(ref _objVersionUpdaterCancellationTokenSource, null, objNewSource);
                    objNewSource.Dispose();
                    throw;
                }

                Task tskNew = Task.Run(async () =>
                {
                    while (true)
                    {
                        objNewToken.ThrowIfCancellationRequested();
                        await DoCacheGitVersion(objNewToken);
                        objNewToken.ThrowIfCancellationRequested();
                        if (Utils.GitUpdateAvailable > 0)
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: objNewToken);
                            string strNewText = Application.ProductName + strSpace + '-' + strSpace +
                                                await LanguageManager.GetStringAsync(
                                                    "String_Version", token: objNewToken)
                                                + strSpace + _strCurrentVersion + strSpace + '-' + strSpace
                                                + string.Format(GlobalSettings.CultureInfo,
                                                                await LanguageManager.GetStringAsync(
                                                                    "String_Update_Available", token: objNewToken),
                                                                Utils.CachedGitVersion);
                            await this.DoThreadSafeAsync(x => x.Text = strNewText, objNewToken);
                            if (GlobalSettings.AutomaticUpdate && _frmUpdate == null)
                            {
                                ChummerUpdater frmUpdater = await this.DoThreadSafeFuncAsync(() => new ChummerUpdater(), objNewToken);
                                if (Interlocked.CompareExchange(ref _frmUpdate, frmUpdater, null) == null)
                                {
                                    Disposed += (sender, args) => frmUpdater.Dispose();
                                    await frmUpdater.DoThreadSafeAsync(x =>
                                    {
                                        x.FormClosed += (o, args) => ResetChummerUpdater(x);
                                        x.SilentMode = true;
                                    }, objNewToken);
                                }
                                else
                                    await frmUpdater.DoThreadSafeAsync(x => x.Close(), objNewToken);
                            }
                        }

                        objNewToken.ThrowIfCancellationRequested();
                        await Task.Delay(TimeSpan.FromHours(1), objNewToken);
                    }
                    // ReSharper disable once FunctionNeverReturns
                }, objNewToken);
                if (Interlocked.CompareExchange(ref _tskVersionUpdate, tskNew, null) != null)
                {
                    Interlocked.CompareExchange(ref _objVersionUpdaterCancellationTokenSource, null, objNewSource);
                    try
                    {
                        objNewSource.Cancel(false);
                    }
                    finally
                    {
                        objNewSource.Dispose();
                    }
                    try
                    {
                        Utils.SafelyRunSynchronously(() => tskNew, token);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }
#endif

        /*
        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
        */

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /*
        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmGMDashboard.Instance.Show();
        }
        */

        private void closeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is Form frmCurrentForm)
                frmCurrentForm.Close();
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                if (childForm != CharacterRoster && childForm != MasterIndex)
                    childForm.Close();
            }
        }

        private async void mnuGlobalSettings_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    using (ThreadSafeForm<EditGlobalSettings> frmOptions
                           = await ThreadSafeForm<EditGlobalSettings>.GetAsync(
                               () => new EditGlobalSettings(), _objGenericToken).ConfigureAwait(false))
                        await frmOptions.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);
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

        private async void mnuCharacterSettings_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    using (ThreadSafeForm<EditCharacterSettings> frmCharacterOptions =
                           await ThreadSafeForm<EditCharacterSettings>.GetAsync(() =>
                               new EditCharacterSettings(
                                   (tabForms.SelectedTab?.Tag as
                                       CharacterShared)
                                   ?.CharacterObject
                                   ?.Settings), _objGenericToken).ConfigureAwait(false))
                        await frmCharacterOptions.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);
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

        private async void mnuToolsUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
                ChummerUpdater frmUpdater = _frmUpdate;
                if (frmUpdater == null)
                {
                    frmUpdater = await this.DoThreadSafeFuncAsync(() => new ChummerUpdater(), _objGenericToken).ConfigureAwait(false);
                    ChummerUpdater objOldUpdater = Interlocked.CompareExchange(ref _frmUpdate, frmUpdater, null);
                    if (objOldUpdater == null)
                    {
                        Disposed += (o, args) => frmUpdater.Dispose();
                        await frmUpdater.DoThreadSafeAsync(x =>
                        {
                            x.FormClosed += (o, args) => ResetChummerUpdater(x);
                            x.Show();
                        }, _objGenericToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await frmUpdater.DoThreadSafeAsync(x => x.Close(), _objGenericToken).ConfigureAwait(false);
                        await objOldUpdater.DoThreadSafeAsync(x =>
                        {
                            if (x.SilentMode)
                            {
                                x.SilentMode = false;
                                x.Show();
                            }
                            else
                            {
                                x.Activate();
                            }
                        }, token: _objGenericToken).ConfigureAwait(false);
                    }
                }
                // Silent updater is running, so make it visible
                else
                {
                    await frmUpdater.DoThreadSafeAsync(x =>
                    {
                        if (x.SilentMode)
                        {
                            x.SilentMode = false;
                            x.Show();
                        }
                        else
                        {
                            x.Activate();
                        }
                    }, token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void mnuMasterIndex_Click(object sender, EventArgs e)
        {
            try
            {
                MasterIndex frmMasterIndex = MasterIndex;
                if (frmMasterIndex.IsNullOrDisposed())
                {
                    frmMasterIndex = await this.DoThreadSafeFuncAsync(() => new MasterIndex(), token: _objGenericToken).ConfigureAwait(false);
                    MasterIndex frmOldMasterIndex
                        = Interlocked.CompareExchange(ref _frmMasterIndex, frmMasterIndex, null);
                    if (frmOldMasterIndex == null)
                    {
                        await frmMasterIndex.DoThreadSafeAsync(x =>
                        {
                            x.FormClosed += (y, args) => Interlocked.CompareExchange(ref _frmMasterIndex, null, x);
                            x.MdiParent = this;
                            bool blnMaximizePreShow = MdiChildren.Length <= 1 && (MdiChildren.Length == 0 || ReferenceEquals(MdiChildren[0], x));
                            Stack<Form> stkToMaximize = null;
                            if (blnMaximizePreShow)
                                x.WindowState = FormWindowState.Maximized;
                            else
                            {
                                // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                stkToMaximize = new Stack<Form>(MdiChildren.Length);
                                stkToMaximize.Push(x);
                                foreach (Form frmLoop in MdiChildren)
                                {
                                    if (frmLoop.WindowState == FormWindowState.Maximized && !ReferenceEquals(frmLoop, x))
                                    {
                                        frmLoop.WindowState = FormWindowState.Normal;
                                        stkToMaximize.Push(frmLoop);
                                    }
                                }
                            }
                            x.Show();
                            if (stkToMaximize?.Count > 0)
                            {
                                while (stkToMaximize.Count > 0)
                                    stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                            }
                        }, token: _objGenericToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await frmMasterIndex.DoThreadSafeAsync(x => x.Close(), _objGenericToken).ConfigureAwait(false);
                        await frmOldMasterIndex.DoThreadSafeAsync(x => x.BringToFront(), _objGenericToken)
                                               .ConfigureAwait(false);
                    }
                }
                else
                {
                    await frmMasterIndex.DoThreadSafeAsync(x =>
                    {
                        foreach (TabPage objTabPage in tabForms.TabPages)
                        {
                            if (objTabPage.Tag != x)
                                continue;
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = null;
                            return;
                        }
                        x.BringToFront();
                    }, token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void mnuCharacterRoster_Click(object sender, EventArgs e)
        {
            try
            {
                CharacterRoster frmCharacterRoster = CharacterRoster;
                if (frmCharacterRoster.IsNullOrDisposed())
                {
                    frmCharacterRoster = await this.DoThreadSafeFuncAsync(() => new CharacterRoster(), token: _objGenericToken).ConfigureAwait(false);
                    CharacterRoster frmOldCharacterRoster
                        = Interlocked.CompareExchange(ref _frmCharacterRoster, frmCharacterRoster, null);
                    if (frmOldCharacterRoster == null)
                    {
                        await frmCharacterRoster.DoThreadSafeAsync(x =>
                        {
                            x.FormClosed += (y, args) => Interlocked.CompareExchange(ref _frmCharacterRoster, null, x);
                            x.MdiParent = this;
                            bool blnMaximizePreShow = MdiChildren.Length <= 1 && (MdiChildren.Length == 0 || ReferenceEquals(MdiChildren[0], x));
                            Stack<Form> stkToMaximize = null;
                            if (blnMaximizePreShow)
                                x.WindowState = FormWindowState.Maximized;
                            else
                            {
                                // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                stkToMaximize = new Stack<Form>(MdiChildren.Length);
                                stkToMaximize.Push(x);
                                foreach (Form frmLoop in MdiChildren)
                                {
                                    if (frmLoop.WindowState == FormWindowState.Maximized && !ReferenceEquals(frmLoop, x))
                                    {
                                        frmLoop.WindowState = FormWindowState.Normal;
                                        stkToMaximize.Push(frmLoop);
                                    }
                                }
                            }
                            x.Show();
                            if (stkToMaximize?.Count > 0)
                            {
                                while (stkToMaximize.Count > 0)
                                    stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                            }
                        }, token: _objGenericToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await frmCharacterRoster.DoThreadSafeAsync(x => x.Close(), _objGenericToken)
                                                .ConfigureAwait(false);
                        await frmOldCharacterRoster.DoThreadSafeAsync(x => x.BringToFront(), _objGenericToken)
                                                   .ConfigureAwait(false);
                    }
                }
                else
                {
                    await frmCharacterRoster.DoThreadSafeAsync(x =>
                    {
                        foreach (TabPage objTabPage in tabForms.TabPages)
                        {
                            if (objTabPage.Tag != x)
                                continue;
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = null;
                            return;
                        }
                        x.BringToFront();
                    }, token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void ResetChummerUpdater(ChummerUpdater frmExistingUpdater)
        {
            if (frmExistingUpdater == null)
                Interlocked.Exchange(ref _frmUpdate, null)?.Close();
            else if (Interlocked.CompareExchange(ref _frmUpdate, null, frmExistingUpdater) == frmExistingUpdater)
                frmExistingUpdater.Close();
        }

        private async void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (ThreadSafeForm<About> frmAbout = await ThreadSafeForm<About>.GetAsync(() => new About(), _objGenericToken).ConfigureAwait(false))
                    await frmAbout.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void mnuChummerWiki_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/chummer5a/chummer5a/wiki/") { UseShellExecute = true });
        }

        private void mnuChummerDiscord_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://discord.gg/mJB7st9") { UseShellExecute = true });
        }

        private void mnuHelpDumpshock_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/chummer5a/chummer5a/issues/") { UseShellExecute = true });
        }

        public PrintMultipleCharacters PrintMultipleCharactersForm { get; private set; }

        private async void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            try
            {
                if (PrintMultipleCharactersForm.IsNullOrDisposed())
                {
                    PrintMultipleCharactersForm = await this.DoThreadSafeFuncAsync(() => new PrintMultipleCharacters(), token: _objGenericToken).ConfigureAwait(false);
                    await PrintMultipleCharactersForm.DoThreadSafeAsync(x => x.Show(), token: _objGenericToken).ConfigureAwait(false);
                }
                else
                    await PrintMultipleCharactersForm.DoThreadSafeAsync(x => x.Activate(), token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
        {
            try
            {
                using (ThreadSafeForm<VersionHistory> frmShowHistory
                       = await ThreadSafeForm<VersionHistory>.GetAsync(() => new VersionHistory(), _objGenericToken).ConfigureAwait(false))
                    await frmShowHistory.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void mnuNewCritter_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Character objCharacter = new Character();
                    try
                    {
                        using (ThreadSafeForm<SelectBuildMethod> frmPickSetting
                               = await ThreadSafeForm<SelectBuildMethod>.GetAsync(
                                   () => new SelectBuildMethod(objCharacter), _objGenericToken).ConfigureAwait(false))
                        {
                            if (await frmPickSetting.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false) == DialogResult.Cancel)
                                return;
                        }

                        // Override the defaults for the setting.
                        objCharacter.IgnoreRules = true;
                        objCharacter.IsCritter = true;
                        await objCharacter.SetCreatedAsync(true, token: _objGenericToken).ConfigureAwait(false);

                        // Show the Metatype selection window.
                        using (ThreadSafeForm<SelectMetatypeKarma> frmSelectMetatype =
                               await ThreadSafeForm<SelectMetatypeKarma>.GetAsync(() =>
                                   new SelectMetatypeKarma(
                                       objCharacter, "critters.xml"), _objGenericToken).ConfigureAwait(false))
                        {
                            if (await frmSelectMetatype.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false) == DialogResult.Cancel)
                                return;
                        }

                        await Program.OpenCharacters.AddAsync(objCharacter, _objGenericToken).ConfigureAwait(false);
                        await OpenCharacter(objCharacter, false, _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objCharacter
                              .DisposeAsync().ConfigureAwait(false); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
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

        private async void mnuMRU_Click(object sender, EventArgs e)
        {
            try
            {
                string strFileName = await mnuProcessFile.DoThreadSafeFuncAsync(() => ((ToolStripMenuItem)sender).Tag, token: _objGenericToken).ConfigureAwait(false) as string;
                if (string.IsNullOrEmpty(strFileName))
                    return;
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    Character objCharacter;
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar
                           = await Program.CreateAndShowProgressBarAsync(strFileName, Character.NumLoadingSections, _objGenericToken).ConfigureAwait(false))
                        objCharacter = await Program.LoadCharacterAsync(strFileName, frmLoadingBar: frmLoadingBar.MyForm, token: _objGenericToken).ConfigureAwait(false);
                    await OpenCharacter(objCharacter, token: _objGenericToken).ConfigureAwait(false);
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

        private async void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            try
            {
                string strFileName = ((ToolStripMenuItem)sender).Tag as string;
                if (!string.IsNullOrEmpty(strFileName))
                    await GlobalSettings.FavoriteCharacters.AddWithSortAsync(strFileName, token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            try
            {
                string strFileName = ((ToolStripMenuItem)sender).Tag as string;
                if (!string.IsNullOrEmpty(strFileName))
                {
                    await GlobalSettings.FavoriteCharacters.RemoveAsync(strFileName, _objGenericToken).ConfigureAwait(false);
                    await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, strFileName, _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ChummerMainForm_MdiChildActivate(object sender, EventArgs e)
        {
            try
            {
                // If there are no child forms, hide the tab control.
                Form objMdiChild = await this.DoThreadSafeFuncAsync(x => x.ActiveMdiChild, token: _objGenericToken).ConfigureAwait(false);
                if (objMdiChild != null)
                {
                    await objMdiChild.DoThreadSafeAsync(x =>
                    {
                        if (x.WindowState == FormWindowState.Minimized)
                        {
                            x.WindowState = FormWindowState.Normal;
                        }
                    }, token: _objGenericToken).ConfigureAwait(false);

                    // If this is a new child form and does not have a tab page, create one.
                    if (!(await objMdiChild.DoThreadSafeFuncAsync(x => x.Tag, token: _objGenericToken).ConfigureAwait(false) is TabPage))
                    {
                        TabPage objTabPage = await this.DoThreadSafeFuncAsync(() => new TabPage
                        {
                            // Add a tab page.
                            Tag = objMdiChild,
                            Parent = tabForms
                        }, token: _objGenericToken).ConfigureAwait(false);

                        switch (objMdiChild)
                        {
                            case CharacterShared frmCharacterShared:
                            {
                                await objTabPage.DoThreadSafeAsync(x => x.Text = frmCharacterShared.CharacterObject.CharacterName, token: _objGenericToken).ConfigureAwait(false);
                                if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                                {
                                    _mascotChummy.CharacterObject = frmCharacterShared.CharacterObject;
                                }

                                break;
                            }
                            case CharacterSheetViewer frmSheetViewer:
                            {
                                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: _objGenericToken).ConfigureAwait(false);
                                string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank", token: _objGenericToken).ConfigureAwait(false);
                                await objTabPage.DoThreadSafeAsync(
                                    x => x.Text = string.Format(
                                        strSheet,
                                        string.Join(',' + strSpace,
                                                    frmSheetViewer.CharacterObjects.Select(y => y.CharacterName.Trim()))), token: _objGenericToken).ConfigureAwait(false);
                                if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                                {
                                    _mascotChummy.CharacterObject = null;
                                }

                                break;
                            }
                            case ExportCharacter frmExportCharacter:
                            {
                                string strExport = await LanguageManager.GetStringAsync("String_Export_Blank", token: _objGenericToken).ConfigureAwait(false);
                                await objTabPage.DoThreadSafeAsync(
                                    x => x.Text = string.Format(
                                        strExport,
                                        frmExportCharacter.CharacterObject.CharacterName.Trim()), token: _objGenericToken).ConfigureAwait(false);
                                if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                                {
                                    _mascotChummy.CharacterObject = null;
                                }

                                break;
                            }
                            default:
                            {
                                string strKey = await objMdiChild.DoThreadSafeFuncAsync(x => x.Tag?.ToString(), token: _objGenericToken).ConfigureAwait(false);
                                if (!string.IsNullOrEmpty(strKey))
                                {
                                    string strTagText
                                        = await LanguageManager.GetStringAsync(strKey, GlobalSettings.Language, false, _objGenericToken).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strTagText))
                                        await objTabPage.DoThreadSafeAsync(x => x.Text = strTagText, token: _objGenericToken).ConfigureAwait(false);
                                }
                                if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                                {
                                    _mascotChummy.CharacterObject = null;
                                }

                                break;
                            }
                        }

                        await tabForms.DoThreadSafeAsync(x =>
                        {
                            if (objMdiChild == MasterIndex && x.TabPages.IndexOf(objTabPage) != 0)
                            {
                                x.TabPages.Remove(objTabPage);
                                x.TabPages.Insert(0, objTabPage);
                            }
                            else if (objMdiChild == CharacterRoster)
                            {
                                if (MasterIndex.IsNullOrDisposed())
                                {
                                    if (x.TabPages.IndexOf(objTabPage) != 0)
                                    {
                                        x.TabPages.Remove(objTabPage);
                                        x.TabPages.Insert(0, objTabPage);
                                    }
                                }
                                else if (x.TabPages.IndexOf(objTabPage) != 1)
                                {
                                    x.TabPages.Remove(objTabPage);
                                    x.TabPages.Insert(1, objTabPage);
                                }
                            }
                            x.SelectedTab = objTabPage;
                        }, token: _objGenericToken).ConfigureAwait(false);

                        await objMdiChild.DoThreadSafeAsync(x =>
                        {
                            x.Tag = objTabPage;
                            x.FormClosed += ActiveMdiChild_FormClosed;
                        }, token: _objGenericToken).ConfigureAwait(false);
                    }
                }
                // Don't show the tab control if there is only one window open.
                await tabForms.DoThreadSafeAsync(x => x.Visible = x.TabCount > 1, token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ActiveMdiChild_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (sender is Form objForm)
                {
                    objForm.FormClosed -= ActiveMdiChild_FormClosed;
                    if (objForm.Tag is TabPage objTabPage)
                    {
                        await tabForms.DoThreadSafeAsync(x =>
                        {
                            if (x.TabCount > 1)
                            {
                                int intSelectTab = x.TabPages.IndexOf(objTabPage);
                                if (intSelectTab > 0)
                                {
                                    if (intSelectTab + 1 >= x.TabCount)
                                        --intSelectTab;
                                    else
                                        ++intSelectTab;
                                    x.SelectedIndex = intSelectTab;
                                }
                            }
                        }, token: _objGenericToken).ConfigureAwait(false);
                        await objTabPage.DoThreadSafeAsync(x => x.Dispose(), token: _objGenericToken).ConfigureAwait(false);
                    }
                }

                await tabForms.DoThreadSafeAsync(x =>
                {
                    // Don't show the tab control if there is only one window open.
                    if (x.TabCount <= 1)
                        x.Visible = false;
                }, token: _objGenericToken).ConfigureAwait(false);

                await DoReopenCharacters(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            (tabForms.SelectedTab?.Tag as Form)?.Select();
        }

        private async Task DoReopenCharacters(CancellationToken token = default)
        {
            if (this.IsNullOrDisposed())
                return;
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                if (Interlocked.Decrement(ref _intSkipReopenUntilAllClear) >= 0)
                {
                    ThreadSafeObservableCollection<CharacterShared> lstToProcess = OpenCharacterEditorForms;
                    if (lstToProcess != null && await lstToProcess.GetCountAsync(token).ConfigureAwait(false) != 0)
                    {
                        Interlocked.Increment(ref _intSkipReopenUntilAllClear);
                        return;
                    }
                }
                else
                    Interlocked.Increment(ref _intSkipReopenUntilAllClear);

                ConcurrentBag<Character> lstLocal = Interlocked.Exchange(ref _lstCharactersToReopen, new ConcurrentBag<Character>());

                await Program.OpenCharacterList(lstLocal, token: token).ConfigureAwait(false);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Switch to an open character form if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenCharacter(Character objCharacter, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (objCharacter == null)
                        return false;
                    ThreadSafeObservableCollection<CharacterShared> lstToProcess = OpenCharacterEditorForms;
                    if (lstToProcess == null)
                        return false;
                    CharacterShared objCharacterForm
                        = await lstToProcess.FirstOrDefaultAsync(
                            x => x.CharacterObject == objCharacter, token).ConfigureAwait(false);
                    if (objCharacterForm == null)
                        return false;
                    await objCharacterForm.DoThreadSafeAsync(x =>
                    {
                        foreach (TabPage objTabPage in tabForms.TabPages)
                        {
                            if (objTabPage.Tag != x)
                                continue;
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = objCharacter;
                            return;
                        }

                        x.BringToFront();
                    }, token).ConfigureAwait(false);

                    return true;
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Switch to an open character sheet viewer if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenPrintCharacter(Character objCharacter, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (objCharacter == null)
                        return false;
                    ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess = OpenCharacterSheetViewers;
                    if (lstToProcess == null)
                        return false;
                    CharacterSheetViewer objCharacterForm
                        = await lstToProcess.FirstOrDefaultAsync(
                            x => x.CharacterObjects.Contains(objCharacter), token).ConfigureAwait(false);
                    if (objCharacterForm == null)
                        return false;
                    await objCharacterForm.DoThreadSafeAsync(x =>
                    {
                        foreach (TabPage objTabPage in tabForms.TabPages)
                        {
                            if (objTabPage.Tag != x)
                                continue;
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = objCharacter;
                            return;
                        }

                        x.BringToFront();
                    }, token).ConfigureAwait(false);
                    return true;
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Switch to an open character exporter if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenExportCharacter(Character objCharacter, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (objCharacter == null)
                        return false;
                    ThreadSafeObservableCollection<ExportCharacter> lstToProcess = OpenCharacterExportForms;
                    if (lstToProcess == null)
                        return false;
                    ExportCharacter objCharacterForm
                        = await lstToProcess.FirstOrDefaultAsync(
                            x => ReferenceEquals(x.CharacterObject, objCharacter), token).ConfigureAwait(false);
                    if (objCharacterForm == null)
                        return false;
                    await objCharacterForm.DoThreadSafeAsync(x =>
                    {
                        foreach (TabPage objTabPage in tabForms.TabPages)
                        {
                            if (objTabPage.Tag != x)
                                continue;
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = objCharacter;
                            return;
                        }

                        x.BringToFront();
                    }, token).ConfigureAwait(false);
                    return true;
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        public async Task UpdateCharacterTabTitle(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
                if (e?.PropertyName == nameof(Character.CharacterName)
                    && sender is Character objCharacter
                    && await tabForms.DoThreadSafeFuncAsync(x => x.TabCount, token: token).ConfigureAwait(false) > 0)
                {
                    await UpdateCharacterTabTitle(objCharacter, objCharacter.CharacterName.Trim(), token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task UpdateCharacterTabTitle(Character objCharacter, string strCharacterName, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank", token: token).ConfigureAwait(false);
                string strExport = await LanguageManager.GetStringAsync("String_Export_Blank", token: token).ConfigureAwait(false);
                await tabForms.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in x.TabPages)
                    {
                        switch (objTabPage.Tag)
                        {
                            case CharacterShared frmCharacter when frmCharacter.CharacterObject == objCharacter:
                                objTabPage.Text = strCharacterName;
                                break;
                            case CharacterSheetViewer frmCharacterSheetViewer
                                when frmCharacterSheetViewer.CharacterObjects.Contains(objCharacter):
                                objTabPage.Text
                                    = string.Format(
                                        strSheet,
                                        string.Join(',' + strSpace,
                                                    frmCharacterSheetViewer.CharacterObjects.Select(
                                                        y => y.CharacterName.Trim())));
                                break;
                            case ExportCharacter frmExport when frmExport.CharacterObject == objCharacter:
                                objTabPage.Text = string.Format(strExport, strCharacterName);
                                break;
                        }
                    }
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        public void RefreshAllTabTitles(CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                string strSpace = LanguageManager.GetString("String_Space", token: token);
                string strSheet = LanguageManager.GetString("String_Sheet_Blank", token: token);
                string strExport = LanguageManager.GetString("String_Export_Blank", token: token);
                tabForms.DoThreadSafe((x, z) =>
                {
                    foreach (TabPage objTabPage in x.TabPages)
                    {
                        switch (objTabPage.Tag)
                        {
                            case CharacterShared frmCharacter:
                                objTabPage.Text = frmCharacter.CharacterObject.CharacterName.Trim();
                                break;
                            case CharacterSheetViewer frmCharacterSheetViewer:
                                objTabPage.Text
                                    = string.Format(
                                        strSheet,
                                        string.Join(',' + strSpace,
                                                    frmCharacterSheetViewer.CharacterObjects.Select(
                                                        y => y.CharacterName.Trim())));
                                break;
                            case ExportCharacter frmExport:
                                objTabPage.Text = string.Format(strExport, frmExport.CharacterObject.CharacterName);
                                break;
                            case Form frmOther:
                                objTabPage.Text = frmOther.Text;
                                break;
                        }
                    }
                }, token: token);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        public async Task RefreshAllTabTitlesAsync(CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank", token: token).ConfigureAwait(false);
                string strExport = await LanguageManager.GetStringAsync("String_Export_Blank", token: token).ConfigureAwait(false);
                await tabForms.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in x.TabPages)
                    {
                        switch (objTabPage.Tag)
                        {
                            case CharacterShared frmCharacter:
                                objTabPage.Text = frmCharacter.CharacterObject.CharacterName.Trim();
                                break;
                            case CharacterSheetViewer frmCharacterSheetViewer:
                                objTabPage.Text
                                    = string.Format(
                                        strSheet,
                                        string.Join(',' + strSpace,
                                                    frmCharacterSheetViewer.CharacterObjects.Select(
                                                        y => y.CharacterName.Trim())));
                                break;
                            case ExportCharacter frmExport:
                                objTabPage.Text = string.Format(strExport, frmExport.CharacterObject.CharacterName);
                                break;
                            case Form frmOther:
                                objTabPage.Text = frmOther.Text;
                                break;
                        }
                    }
                }, token).ConfigureAwait(false);
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private async void mnuToolsDiceRoller_Click(object sender, EventArgs e)
        {
            try
            {
                await OpenDiceRollerWithPool(token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void menuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            try
            {
                // Translate the items in the menu by finding their Tags in the translation file.
                await menuStrip.DoThreadSafeAsync(x =>
                {
                    foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                    {
                        tssItem.UpdateLightDarkMode(token: _objGenericToken);
                    }
                }, token: _objGenericToken).ConfigureAwait(false);
                List<Tuple<ToolStripItem, string>> lstToTranslate
                    = new List<Tuple<ToolStripItem, string>>();
                foreach (ToolStripItem tssItem in await menuStrip
                                                        .DoThreadSafeFuncAsync(
                                                            (x, y) => x.Items, _objGenericToken)
                                                        .ConfigureAwait(false))
                    lstToTranslate.AddRange(
                        await menuStrip.TranslateToolStripItemsRecursivelyPrepAsync(
                            tssItem, token: _objGenericToken).ConfigureAwait(false));
                foreach ((ToolStripItem objControl, string strTag) in lstToTranslate)
                {
                    string strText = await LanguageManager.GetStringAsync(strTag, token: _objGenericToken).ConfigureAwait(false);
                    await menuStrip.DoThreadSafeAsync(() => objControl.Text = strText, token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void RefreshToolStripDisplays(object sender, ToolStripItemEventArgs e)
        {
            try
            {
                // ToolStrip Items.
                foreach (ToolStrip objToolStrip in await this.DoThreadSafeFuncAsync((x, y) => x.Controls.OfType<ToolStrip>(), _objGenericToken).ConfigureAwait(false))
                {
                    await objToolStrip.DoThreadSafeAsync(x =>
                    {
                        foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                        {
                            tssItem.UpdateLightDarkMode(token: _objGenericToken);
                        }
                    }, token: _objGenericToken).ConfigureAwait(false);
                    List<Tuple<ToolStripItem, string>> lstToTranslate
                        = new List<Tuple<ToolStripItem, string>>();
                    foreach (ToolStripItem tssItem in await objToolStrip
                                                            .DoThreadSafeFuncAsync(
                                                                (x, y) => x.Items, _objGenericToken)
                                                            .ConfigureAwait(false))
                        lstToTranslate.AddRange(
                            await objToolStrip.TranslateToolStripItemsRecursivelyPrepAsync(
                                tssItem, token: _objGenericToken).ConfigureAwait(false));
                    foreach ((ToolStripItem objControl, string strTag) in lstToTranslate)
                    {
                        string strText = await LanguageManager.GetStringAsync(strTag, token: _objGenericToken).ConfigureAwait(false);
                        await objToolStrip.DoThreadSafeAsync(() => objControl.Text = strText, token: _objGenericToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private bool IsVisibleOnAnyScreen()
        {
            Rectangle objMyRectangle = ClientRectangle;
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(objMyRectangle));
        }

        private async void ChummerMainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    // Open each file that has been dropped into the window.
                    string[] s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                    if (s.Length == 0)
                        return;
                    Character[] lstCharacters = new Character[s.Length];
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar
                           = await Program.CreateAndShowProgressBarAsync(string.Empty,
                                                                         Character.NumLoadingSections * s.Length, _objGenericToken).ConfigureAwait(false))
                    {
                        Task<Character>[] tskCharacterLoads = new Task<Character>[s.Length];
                        // Array instead of concurrent bag because we want to preserve order
                        for (int i = 0; i < s.Length; ++i)
                        {
                            string strFile = s[i];
                            // ReSharper disable once AccessToDisposedClosure
                            tskCharacterLoads[i]
                                = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm, token: _objGenericToken), _objGenericToken);
                        }

                        await Task.WhenAll(tskCharacterLoads).ConfigureAwait(false);
                        for (int i = 0; i < lstCharacters.Length; ++i)
                            lstCharacters[i] = await tskCharacterLoads[i].ConfigureAwait(false);
                    }

                    await OpenCharacterList(lstCharacters, token: _objGenericToken).ConfigureAwait(false);
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

        private void ChummerMainForm_DragEnter(object sender, DragEventArgs e)
        {
            // Only use a drop effect if a file is being dragged into the window.
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void mnuToolsTranslator_Click(object sender, EventArgs e)
        {
            string strTranslator = Path.Combine(Utils.GetStartupPath, "Translator.exe");
            if (File.Exists(strTranslator))
                Process.Start(new ProcessStartInfo(strTranslator) { UseShellExecute = true });
        }

        private async void ChummerMainForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (Interlocked.Exchange(ref _intFormClosing, 1) == 1)
                return;
            _objGenericCancellationTokenSource.Cancel(false);
            Program.OpenCharacters.CollectionChanged -= OpenCharactersOnCollectionChanged;
            foreach (Character objCharacter in Program.OpenCharacters)
            {
                if (objCharacter?.IsDisposed == false)
                {
                    try
                    {
                        objCharacter.PropertyChangedAsync -= UpdateCharacterTabTitle;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
            }

#if !DEBUG
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objVersionUpdaterCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            Task tskOld = Interlocked.Exchange(ref _tskVersionUpdate, null);
            if (tskOld != null)
            {
                try
                {
                    await tskOld;
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
#endif
            await DisposeOpenFormsAsync().ConfigureAwait(false);

            FormWindowState eWindowState = await this.DoThreadSafeFuncAsync(x => x.WindowState, CancellationToken.None)
                                                     .ConfigureAwait(false);
            Properties.Settings.Default.WindowState = eWindowState;
            if (eWindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = await this
                                                             .DoThreadSafeFuncAsync(
                                                                 x => x.Location, CancellationToken.None)
                                                             .ConfigureAwait(false);
                Properties.Settings.Default.Size = await this.DoThreadSafeFuncAsync(x => x.Size, CancellationToken.None)
                                                             .ConfigureAwait(false);
            }
            else
            {
                Properties.Settings.Default.Location = await this
                                                             .DoThreadSafeFuncAsync(
                                                                 x => x.RestoreBounds.Location, CancellationToken.None)
                                                             .ConfigureAwait(false);
                Properties.Settings.Default.Size = await this
                                                         .DoThreadSafeFuncAsync(
                                                             x => x.RestoreBounds.Size, CancellationToken.None)
                                                         .ConfigureAwait(false);
            }

            try
            {
                Properties.Settings.Default.Save();
            }
            catch (IOException ex)
            {
                Log.Warn(ex, ex.Message);
            }
        }

        private void DisposeOpenForms()
        {
            ThreadSafeObservableCollection<CharacterShared> lstToClose1
                = Interlocked.Exchange(ref _lstOpenCharacterEditorForms, null);
            if (lstToClose1 != null)
            {
                using (lstToClose1.LockObject.EnterWriteLock())
                {
                    for (int i = lstToClose1.Count - 1; i >= 0; --i)
                    {
                        CharacterShared frmToClose = lstToClose1[i];
                        Character objFormCharacter = frmToClose.CharacterObject;
                        frmToClose.DoThreadSafe(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        });
                        objFormCharacter.Dispose();
                    }
                }

                lstToClose1.Dispose();
            }

            ThreadSafeObservableCollection<ExportCharacter> lstToClose2
                = Interlocked.Exchange(ref _lstOpenCharacterExportForms, null);
            if (lstToClose2 != null)
            {
                using (lstToClose2.LockObject.EnterWriteLock())
                {
                    for (int i = lstToClose2.Count - 1; i >= 0; --i)
                    {
                        ExportCharacter frmToClose = lstToClose2[i];
                        Character objFormCharacter = frmToClose.CharacterObject;
                        frmToClose.DoThreadSafe(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        });
                        objFormCharacter.Dispose();
                    }
                }

                lstToClose2.Dispose();
            }

            ThreadSafeObservableCollection<CharacterSheetViewer> lstToClose3
                = Interlocked.Exchange(ref _lstOpenCharacterSheetViewers, null);
            if (lstToClose3 != null)
            {
                using (lstToClose3.LockObject.EnterWriteLock())
                {
                    for (int i = lstToClose3.Count - 1; i >= 0; --i)
                    {
                        CharacterSheetViewer frmToClose = lstToClose3[i];
                        List<Character> lstFormCharacters = frmToClose.CharacterObjects.ToList();
                        frmToClose.DoThreadSafe(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        });
                        foreach (Character objFormCharacter in lstFormCharacters)
                            objFormCharacter.Dispose();
                    }
                }

                lstToClose3.Dispose();
            }
        }

        private async ValueTask DisposeOpenFormsAsync()
        {
            ThreadSafeObservableCollection<CharacterShared> lstToClose1
                = Interlocked.Exchange(ref _lstOpenCharacterEditorForms, null);
            if (lstToClose1 != null)
            {
                // ReSharper disable once MethodSupportsCancellation
                IAsyncDisposable objLocker = await lstToClose1.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    for (int i = await lstToClose1.GetCountAsync()
                             .ConfigureAwait(false) - 1;
                         i >= 0;
                         --i)
                    {
                        CharacterShared frmToClose = await lstToClose1
                            // ReSharper disable once MethodSupportsCancellation
                            .GetValueAtAsync(i)
                                                           .ConfigureAwait(false);
                        Character objFormCharacter = frmToClose.CharacterObject;
                        // ReSharper disable once MethodSupportsCancellation
                        await frmToClose.DoThreadSafeAsync(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        }).ConfigureAwait(false);
                        await objFormCharacter.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                await lstToClose1.DisposeAsync().ConfigureAwait(false);
            }

            ThreadSafeObservableCollection<ExportCharacter> lstToClose2
                = Interlocked.Exchange(ref _lstOpenCharacterExportForms, null);
            if (lstToClose2 != null)
            {
                // ReSharper disable once MethodSupportsCancellation
                IAsyncDisposable objLocker = await lstToClose2.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    for (int i = await lstToClose2.GetCountAsync().ConfigureAwait(false) - 1;
                         i >= 0;
                         --i)
                    {
                        ExportCharacter frmToClose = await lstToClose2
                            // ReSharper disable once MethodSupportsCancellation
                            .GetValueAtAsync(i)
                                                           .ConfigureAwait(false);
                        Character objFormCharacter = frmToClose.CharacterObject;
                        // ReSharper disable once MethodSupportsCancellation
                        await frmToClose.DoThreadSafeAsync(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        }).ConfigureAwait(false);
                        await objFormCharacter.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                await lstToClose2.DisposeAsync().ConfigureAwait(false);
            }

            ThreadSafeObservableCollection<CharacterSheetViewer> lstToClose3
                = Interlocked.Exchange(ref _lstOpenCharacterSheetViewers, null);
            if (lstToClose3 != null)
            {
                // ReSharper disable once MethodSupportsCancellation
                IAsyncDisposable objLocker = await lstToClose3.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    // ReSharper disable once MethodSupportsCancellation
                    for (int i = await lstToClose3.GetCountAsync()
                             .ConfigureAwait(false) - 1;
                         i >= 0;
                         --i)
                    {
                        CharacterSheetViewer frmToClose = await lstToClose3
                            // ReSharper disable once MethodSupportsCancellation
                            .GetValueAtAsync(i)
                                                                .ConfigureAwait(false);
                        List<Character> lstFormCharacters = frmToClose.CharacterObjects.ToList();
                        // ReSharper disable once MethodSupportsCancellation
                        await frmToClose.DoThreadSafeAsync(x =>
                        {
                            try
                            {
                                x.Close();
                            }
                            catch (ObjectDisposedException)
                            {
                                // swallow this
                            }
                        }).ConfigureAwait(false);
                        foreach (Character objFormCharacter in lstFormCharacters)
                            await objFormCharacter.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                await lstToClose3.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void mnuHeroLabImporter_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.ShowScrollableMessageBox(await LanguageManager.GetStringAsync("Message_HeroLabImporterWarning", token: _objGenericToken).ConfigureAwait(false),
                                                     await LanguageManager.GetStringAsync("Message_HeroLabImporterWarning_Title", token: _objGenericToken).ConfigureAwait(false),
                                                     MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                HeroLabImporter frmImporter = await this.DoThreadSafeFuncAsync(() => new HeroLabImporter(), token: _objGenericToken).ConfigureAwait(false);
                await frmImporter.DoThreadSafeAsync(x => x.Show(), token: _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void tabForms_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabForms.TabCount; ++i)
                {
                    if (!tabForms.GetTabRect(i).Contains(e.Location))
                        continue;
                    if (tabForms.SelectedTab.Tag is CharacterShared && tabForms.SelectedIndex == i)
                    {
                        mnuProcessFile.Show(this, e.Location);
                        break;
                    }
                }
            }
        }

        private async void tsSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag, token: _objGenericToken).ConfigureAwait(false) is CharacterShared objShared
                    && await objShared.SaveCharacter(token: _objGenericToken).ConfigureAwait(false) && objShared is CharacterCreate objCreate
                    && objCreate.IsReopenQueued)
                {
                    await objCreate.DoThreadSafeAsync(x => x.Close(), token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void tsSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag, token: _objGenericToken).ConfigureAwait(false) is CharacterShared objShared
                    && await objShared.SaveCharacterAs(token: _objGenericToken).ConfigureAwait(false) && objShared is CharacterCreate objCreate
                    && objCreate.IsReopenQueued)
                {
                    await objCreate.DoThreadSafeAsync(x => x.Close(), token: _objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void tsClose_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.Close();
            }
        }

        private async void tsPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag, token: _objGenericToken).ConfigureAwait(false) is CharacterShared objShared)
                {
                    await objShared.DoPrint(_objGenericToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void ChummerMainForm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            tabForms.ItemSize = new Size(
                tabForms.ItemSize.Width * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1),
                tabForms.ItemSize.Height * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1));
        }

#if DEBUG
        private void mnuForceCrash_Click(object sender, EventArgs e)
        {
            throw new InvalidOperationException();
        }
#endif

#endregion Control Events

#region Methods
        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private async void ShowNewForm(object sender, EventArgs e)
        {
            try
            {
                Character objCharacter = new Character();
                try
                {
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        // Show the BP selection window.
                        using (ThreadSafeForm<SelectBuildMethod> frmBP
                               = await ThreadSafeForm<SelectBuildMethod>.GetAsync(
                                   () => new SelectBuildMethod(objCharacter), _objGenericToken).ConfigureAwait(false))
                        {
                            if (await frmBP.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false) != DialogResult.OK)
                                return;
                        }

                        // Show the Metatype selection window.
                        if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
                        {
                            using (ThreadSafeForm<SelectMetatypePriority> frmSelectMetatype
                                   = await ThreadSafeForm<SelectMetatypePriority>.GetAsync(
                                       () => new SelectMetatypePriority(objCharacter), _objGenericToken).ConfigureAwait(false))
                            {
                                if (await frmSelectMetatype.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false) != DialogResult.OK)
                                    return;
                            }
                        }
                        else
                        {
                            using (ThreadSafeForm<SelectMetatypeKarma> frmSelectMetatype
                                   = await ThreadSafeForm<SelectMetatypeKarma>.GetAsync(
                                       () => new SelectMetatypeKarma(objCharacter), _objGenericToken).ConfigureAwait(false))
                            {
                                if (await frmSelectMetatype.ShowDialogSafeAsync(this, _objGenericToken).ConfigureAwait(false) != DialogResult.OK)
                                    return;
                            }
                        }

                        await Program.OpenCharacters.AddAsync(objCharacter, _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }

                    objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                    try
                    {
                        await this.DoThreadSafeAsync(x =>
                        {
                            CharacterCreate frmNewCharacter = new CharacterCreate(objCharacter)
                            {
                                MdiParent = x
                            };
                            bool blnMaximizePreShow = x.MdiChildren.Length <= 1 && (x.MdiChildren.Length == 0 || ReferenceEquals(MdiChildren[0], frmNewCharacter));
                            Stack<Form> stkToMaximize = null;
                            if (blnMaximizePreShow)
                                frmNewCharacter.WindowState = FormWindowState.Maximized;
                            else
                            {
                                // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                stkToMaximize = new Stack<Form>(x.MdiChildren.Length);
                                stkToMaximize.Push(frmNewCharacter);
                                foreach (Form frmLoop in x.MdiChildren)
                                {
                                    if (frmLoop.WindowState == FormWindowState.Maximized && !ReferenceEquals(frmLoop, frmNewCharacter))
                                    {
                                        frmLoop.WindowState = FormWindowState.Normal;
                                        stkToMaximize.Push(frmLoop);
                                    }
                                }
                            }
                            frmNewCharacter.Show();
                            if (stkToMaximize?.Count > 0)
                            {
                                while (stkToMaximize.Count > 0)
                                    stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                            }
                        }, token: _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private async void OpenFile(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x), token: _objGenericToken).ConfigureAwait(false)
                        != DialogResult.OK)
                        return;
                    List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                    foreach (string strFile in dlgOpenFile.FileNames)
                    {
                        Character objLoopCharacter
                            = await Program.OpenCharacters.FirstOrDefaultAsync(
                                x => x.FileName == strFile, token: _objGenericToken).ConfigureAwait(false);
                        if (objLoopCharacter != null)
                        {
                            if (!await SwitchToOpenCharacter(objLoopCharacter, _objGenericToken).ConfigureAwait(false))
                                await OpenCharacter(objLoopCharacter, token: _objGenericToken).ConfigureAwait(false);
                        }
                        else
                            lstFilesToOpen.Add(strFile);
                    }

                    if (lstFilesToOpen.Count == 0)
                        return;
                    // Array instead of concurrent bag because we want to preserve order
                    Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                               string.Join(
                                   ',' + await LanguageManager.GetStringAsync("String_Space", token: _objGenericToken).ConfigureAwait(false),
                                   lstFilesToOpen.Select(Path.GetFileName)),
                               lstFilesToOpen.Count * Character.NumLoadingSections, _objGenericToken).ConfigureAwait(false))
                    {
                        Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                        for (int i = 0; i < lstFilesToOpen.Count; ++i)
                        {
                            string strFile = lstFilesToOpen[i];
                            // ReSharper disable once AccessToDisposedClosure
                            tskCharacterLoads[i]
                                = Task.Run(
                                    () => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm,
                                                                     token: _objGenericToken), _objGenericToken);
                        }

                        await Task.WhenAll(tskCharacterLoads).ConfigureAwait(false);
                        for (int i = 0; i < lstCharacters.Length; ++i)
                            lstCharacters[i] = await tskCharacterLoads[i].ConfigureAwait(false);
                    }

                    await OpenCharacterList(lstCharacters, token: _objGenericToken).ConfigureAwait(false);
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

            //Timekeeper.Finish("load_sum");
            //Timekeeper.Log();
        }

        /// <summary>
        /// Opens the correct window for a single character.
        /// </summary>
        public Task OpenCharacter(Character objCharacter, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            return OpenCharacterList(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open the correct windows for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (lstCharacters == null)
                        return;
                    List<Character> lstNewCharacters = lstCharacters.ToList();
                    if (lstNewCharacters.Count == 0)
                        return;
                    await _objFormOpeningSemaphore.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        bool blnMaximizeNewForm
                            = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                                    || x.MdiChildren.Any(
                                                                        y => y.WindowState
                                                                             == FormWindowState.Maximized),
                                                               token).ConfigureAwait(false);
                        string strUI = await LanguageManager.GetStringAsync("String_UI", token: token)
                                                            .ConfigureAwait(false);
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                               .ConfigureAwait(false);
                        string strTooManyHandles
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        string strTooManyHandlesTitle
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token)
                                              .ConfigureAwait(false))
                        {
                            foreach (Character objCharacter in lstNewCharacters)
                            {
                                token.ThrowIfCancellationRequested();
                                await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                                ? strUI
                                                                                : strUI + strSpace + '('
                                                                                + objCharacter.CharacterName
                                                                                + ')', token: token)
                                                   .ConfigureAwait(false);
                                if (objCharacter == null)
                                    continue;
                                ThreadSafeObservableCollection<CharacterShared> lstToProcess = OpenCharacterEditorForms;
                                if (lstToProcess != null && await lstToProcess.AnyAsync(
                                        x => x.CharacterObject == objCharacter, token).ConfigureAwait(false))
                                    continue;
                                if (Program.MyProcess.HandleCount >= (objCharacter.Created ? 8000 : 7500)
                                    && Program.ShowScrollableMessageBox(
                                        string.Format(strTooManyHandles, objCharacter.CharacterName),
                                        strTooManyHandlesTitle,
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                                {
                                    if (await Program.OpenCharacters
                                            .AllAsync(
                                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter),
                                                token).ConfigureAwait(false))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token)
                                            .ConfigureAwait(false);
                                    continue;
                                }

                                //Timekeeper.Start("load_event_time");
                                // Show the character forms.
                                await this.DoThreadSafeAsync(y =>
                                {
                                    CharacterShared frmNewCharacter = objCharacter.Created
                                        ? (CharacterShared) new CharacterCareer(objCharacter)
                                        : new CharacterCreate(objCharacter);
                                    frmNewCharacter.MdiParent = y;
                                    bool blnMaximizePreShow = y.MdiChildren.Length <= 1 && (y.MdiChildren.Length == 0
                                        || ReferenceEquals(MdiChildren[0], frmNewCharacter));
                                    Stack<Form> stkToMaximize = null;
                                    if (blnMaximizePreShow)
                                    {
                                        if (blnMaximizeNewForm)
                                            frmNewCharacter.WindowState = FormWindowState.Maximized;
                                    }
                                    else
                                    {
                                        // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                        // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                        stkToMaximize = new Stack<Form>(y.MdiChildren.Length);
                                        if (blnMaximizeNewForm)
                                            stkToMaximize.Push(frmNewCharacter);
                                        foreach (Form frmLoop in y.MdiChildren)
                                        {
                                            if (frmLoop.WindowState == FormWindowState.Maximized
                                                && !ReferenceEquals(frmLoop, frmNewCharacter))
                                            {
                                                frmLoop.WindowState = FormWindowState.Normal;
                                                stkToMaximize.Push(frmLoop);
                                            }
                                        }
                                    }

                                    frmNewCharacter.Show();
                                    if (stkToMaximize?.Count > 0)
                                    {
                                        while (stkToMaximize.Count > 0)
                                            stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                                    }
                                }, token).ConfigureAwait(false);
                                if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                                    && File.Exists(objCharacter.FileName))
                                    await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(
                                        0, objCharacter.FileName, token).ConfigureAwait(false);
                                //Timekeeper.Finish("load_event_time");
                            }
                        }
                    }
                    finally
                    {
                        _objFormOpeningSemaphore.Release();
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private async void OpenFileForPrinting(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x), token: _objGenericToken).ConfigureAwait(false)
                        != DialogResult.OK)
                        return;
                    List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                    foreach (string strFile in dlgOpenFile.FileNames)
                    {
                        Character objLoopCharacter
                            = await Program.OpenCharacters.FirstOrDefaultAsync(
                                x => x.FileName == strFile, token: _objGenericToken).ConfigureAwait(false);
                        if (objLoopCharacter != null)
                        {
                            if (!await SwitchToOpenPrintCharacter(objLoopCharacter, _objGenericToken).ConfigureAwait(false))
                                await OpenCharacterForPrinting(objLoopCharacter, token: _objGenericToken).ConfigureAwait(false);
                        }
                        else
                            lstFilesToOpen.Add(strFile);
                    }

                    if (lstFilesToOpen.Count == 0)
                        return;
                    // Array instead of concurrent bag because we want to preserve order
                    Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                               string.Join(
                                   ',' + await LanguageManager.GetStringAsync("String_Space", token: _objGenericToken).ConfigureAwait(false),
                                   lstFilesToOpen.Select(Path.GetFileName)),
                               lstFilesToOpen.Count * Character.NumLoadingSections, _objGenericToken).ConfigureAwait(false))
                    {
                        Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                        for (int i = 0; i < lstFilesToOpen.Count; ++i)
                        {
                            string strFile = lstFilesToOpen[i];
                            // ReSharper disable once AccessToDisposedClosure
                            tskCharacterLoads[i]
                                = Task.Run(
                                    () => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm,
                                                                     token: _objGenericToken), _objGenericToken);
                        }

                        await Task.WhenAll(tskCharacterLoads).ConfigureAwait(false);
                        for (int i = 0; i < lstCharacters.Length; ++i)
                            lstCharacters[i] = await tskCharacterLoads[i].ConfigureAwait(false);
                    }

                    await OpenCharacterListForPrinting(lstCharacters, token: _objGenericToken).ConfigureAwait(false);
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

        /// <summary>
        /// Open a character's print form up without necessarily opening them up fully for editing.
        /// </summary>
        public Task OpenCharacterForPrinting(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            return OpenCharacterListForPrinting(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open print forms for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterListForPrinting(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (lstCharacters == null)
                        return;
                    List<Character> lstNewCharacters = lstCharacters.ToList();
                    if (lstNewCharacters.Count == 0)
                        return;
                    await _objFormOpeningSemaphore.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        bool blnMaximizeNewForm
                            = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                                    || x.MdiChildren.Any(
                                                                        y => y.WindowState
                                                                             == FormWindowState.Maximized),
                                                               token).ConfigureAwait(false);
                        List<Tuple<CharacterSheetViewer, Character>> lstNewFormsToProcess
                            = new List<Tuple<CharacterSheetViewer, Character>>(lstNewCharacters.Count);
                        string strUI = await LanguageManager.GetStringAsync("String_UI", token: token)
                                                            .ConfigureAwait(false);
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                               .ConfigureAwait(false);
                        string strTooManyHandles
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        string strTooManyHandlesTitle
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token)
                                              .ConfigureAwait(false))
                        {
                            foreach (Character objCharacter in lstNewCharacters)
                            {
                                token.ThrowIfCancellationRequested();
                                await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                                ? strUI
                                                                                : strUI + strSpace + '('
                                                                                + objCharacter.CharacterName
                                                                                + ')', token: token)
                                                   .ConfigureAwait(false);
                                if (objCharacter == null)
                                    continue;
                                ThreadSafeObservableCollection<CharacterSheetViewer> lstToProcess
                                    = OpenCharacterSheetViewers;
                                if (lstToProcess != null && await lstToProcess.AnyAsync(
                                        x => x.CharacterObjects.Contains(objCharacter), token).ConfigureAwait(false))
                                    continue;

                                if (Program.MyProcess.HandleCount >= 9500
                                    && Program.ShowScrollableMessageBox(
                                        string.Format(strTooManyHandles, objCharacter.CharacterName),
                                        strTooManyHandlesTitle,
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                                {
                                    if (await Program.OpenCharacters
                                            .AllAsync(
                                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter),
                                                token).ConfigureAwait(false))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token)
                                            .ConfigureAwait(false);
                                    continue;
                                }

                                //Timekeeper.Start("load_event_time");
                                // Show the character forms.
                                await this.DoThreadSafeAsync(y =>
                                {
                                    CharacterSheetViewer frmViewer = new CharacterSheetViewer
                                    {
                                        MdiParent = y
                                    };
                                    lstNewFormsToProcess.Add(
                                        new Tuple<CharacterSheetViewer, Character>(frmViewer, objCharacter));
                                }, token: token).ConfigureAwait(false);

                                if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                                    && File.Exists(objCharacter.FileName))
                                    await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(
                                        0, objCharacter.FileName, token).ConfigureAwait(false);
                                //Timekeeper.Finish("load_event_time");
                            }
                        }

                        foreach ((CharacterSheetViewer frmViewer, Character objCharacter) in lstNewFormsToProcess)
                        {
                            await frmViewer.SetCharacters(token, objCharacter).ConfigureAwait(false);
                        }

                        await this.DoThreadSafeAsync(x =>
                        {
                            bool blnMaximizePreShow = x.MdiChildren.Length <= 1 && (x.MdiChildren.Length == 0
                                || ReferenceEquals(x.MdiChildren[0], lstNewFormsToProcess[0].Item1));
                            Stack<Form> stkToMaximize = null;
                            if (blnMaximizePreShow)
                            {
                                if (blnMaximizeNewForm)
                                {
                                    foreach ((CharacterSheetViewer frmViewer, Character _) in lstNewFormsToProcess)
                                    {
                                        frmViewer.WindowState = FormWindowState.Maximized;
                                    }
                                }
                            }
                            else
                            {
                                // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                stkToMaximize = new Stack<Form>(x.MdiChildren.Length);
                                if (blnMaximizeNewForm)
                                {
                                    foreach ((CharacterSheetViewer frmViewer, Character _) in lstNewFormsToProcess)
                                    {
                                        stkToMaximize.Push(frmViewer);
                                    }
                                }

                                foreach (Form frmLoop in x.MdiChildren)
                                {
                                    if (frmLoop.WindowState == FormWindowState.Maximized
                                        && lstNewFormsToProcess.All(z => !ReferenceEquals(z.Item1, frmLoop)))
                                    {
                                        frmLoop.WindowState = FormWindowState.Normal;
                                        stkToMaximize.Push(frmLoop);
                                    }
                                }
                            }

                            foreach ((CharacterSheetViewer frmViewer, Character _) in lstNewFormsToProcess)
                                frmViewer.Show();
                            if (stkToMaximize?.Count > 0)
                            {
                                while (stkToMaximize.Count > 0)
                                    stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                            }
                        }, _objGenericToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _objFormOpeningSemaphore.Release();
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private async void OpenFileForExport(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: _objGenericToken).ConfigureAwait(false);
                try
                {
                    if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x), token: _objGenericToken).ConfigureAwait(false)
                        != DialogResult.OK)
                        return;
                    List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                    foreach (string strFile in dlgOpenFile.FileNames)
                    {
                        Character objLoopCharacter
                            = await Program.OpenCharacters.FirstOrDefaultAsync(
                                x => x.FileName == strFile, token: _objGenericToken).ConfigureAwait(false);
                        if (objLoopCharacter != null)
                        {
                            if (!await SwitchToOpenExportCharacter(objLoopCharacter, _objGenericToken).ConfigureAwait(false))
                                await OpenCharacterForExport(objLoopCharacter, token: _objGenericToken).ConfigureAwait(false);
                        }
                        else
                            lstFilesToOpen.Add(strFile);
                    }

                    if (lstFilesToOpen.Count == 0)
                        return;
                    // Array instead of concurrent bag because we want to preserve order
                    Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                               string.Join(
                                   ',' + await LanguageManager.GetStringAsync("String_Space", token: _objGenericToken).ConfigureAwait(false),
                                   lstFilesToOpen.Select(Path.GetFileName)),
                               lstFilesToOpen.Count * Character.NumLoadingSections, _objGenericToken).ConfigureAwait(false))
                    {
                        Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                        for (int i = 0; i < lstFilesToOpen.Count; ++i)
                        {
                            string strFile = lstFilesToOpen[i];
                            // ReSharper disable once AccessToDisposedClosure
                            tskCharacterLoads[i]
                                = Task.Run(
                                    () => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm,
                                                                     token: _objGenericToken), _objGenericToken);
                        }

                        await Task.WhenAll(tskCharacterLoads).ConfigureAwait(false);
                        for (int i = 0; i < lstCharacters.Length; ++i)
                            lstCharacters[i] = await tskCharacterLoads[i].ConfigureAwait(false);
                    }

                    await OpenCharacterListForExport(lstCharacters, token: _objGenericToken).ConfigureAwait(false);
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

        /// <summary>
        /// Open a character's export form up without necessarily opening them up fully for editing.
        /// </summary>
        public Task OpenCharacterForExport(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            return OpenCharacterListForExport(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open export forms for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterListForExport(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (lstCharacters == null)
                        return;
                    List<Character> lstNewCharacters = lstCharacters.ToList();
                    if (lstNewCharacters.Count == 0)
                        return;
                    await _objFormOpeningSemaphore.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        bool blnMaximizeNewForm
                            = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                                    || x.MdiChildren.Any(
                                                                        y => y.WindowState
                                                                             == FormWindowState.Maximized),
                                                               token).ConfigureAwait(false);
                        string strUI = await LanguageManager.GetStringAsync("String_UI", token: token)
                                                            .ConfigureAwait(false);
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                               .ConfigureAwait(false);
                        string strTooManyHandles
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        string strTooManyHandlesTitle
                            = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token)
                                                   .ConfigureAwait(false);
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token)
                                              .ConfigureAwait(false))
                        {
                            foreach (Character objCharacter in lstNewCharacters)
                            {
                                await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                                ? strUI
                                                                                : strUI + strSpace + '('
                                                                                + objCharacter.CharacterName
                                                                                + ')', token: token)
                                                   .ConfigureAwait(false);
                                if (objCharacter == null)
                                    continue;
                                ThreadSafeObservableCollection<ExportCharacter> lstToProcess = OpenCharacterExportForms;
                                if (lstToProcess != null && await lstToProcess.AnyAsync(
                                        x => x.CharacterObject == objCharacter, token).ConfigureAwait(false))
                                    continue;
                                if (Program.MyProcess.HandleCount >= 9500
                                    && Program.ShowScrollableMessageBox(
                                        string.Format(strTooManyHandles, objCharacter.CharacterName),
                                        strTooManyHandlesTitle,
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                                {
                                    if (await Program.OpenCharacters
                                            .AllAsync(
                                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter),
                                                token).ConfigureAwait(false))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter, token)
                                            .ConfigureAwait(false);
                                    continue;
                                }

                                //Timekeeper.Start("load_event_time");
                                // Show the character forms.
                                await this.DoThreadSafeAsync(y =>
                                {
                                    ExportCharacter frmViewer = new ExportCharacter(objCharacter)
                                    {
                                        MdiParent = y
                                    };
                                    bool blnMaximizePreShow = y.MdiChildren.Length <= 1 && (y.MdiChildren.Length == 0
                                        || ReferenceEquals(MdiChildren[0], frmViewer));
                                    Stack<Form> stkToMaximize = null;
                                    if (blnMaximizePreShow)
                                    {
                                        if (blnMaximizeNewForm)
                                            frmViewer.WindowState = FormWindowState.Maximized;
                                    }
                                    else
                                    {
                                        // There is an issue in WinForms MDI Containers where showing a new form when other forms are maximized can cause a crash,
                                        // so let's make sure we un-maximize all maximized forms before showing this newly added one
                                        stkToMaximize = new Stack<Form>(y.MdiChildren.Length);
                                        if (blnMaximizeNewForm)
                                            stkToMaximize.Push(frmViewer);
                                        foreach (Form frmLoop in y.MdiChildren)
                                        {
                                            if (frmLoop.WindowState == FormWindowState.Maximized
                                                && !ReferenceEquals(frmLoop, frmViewer))
                                            {
                                                frmLoop.WindowState = FormWindowState.Normal;
                                                stkToMaximize.Push(frmLoop);
                                            }
                                        }
                                    }

                                    frmViewer.Show();
                                    if (stkToMaximize?.Count > 0)
                                    {
                                        while (stkToMaximize.Count > 0)
                                            stkToMaximize.Pop().WindowState = FormWindowState.Maximized;
                                    }
                                }, token).ConfigureAwait(false);
                                if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                                    && File.Exists(objCharacter.FileName))
                                    await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(
                                        0, objCharacter.FileName, token).ConfigureAwait(false);
                                //Timekeeper.Finish("load_event_time");
                            }
                        }
                    }
                    finally
                    {
                        _objFormOpeningSemaphore.Release();
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private async Task ProcessQueuedCharactersToOpen(CancellationToken token = default)
        {
            ConcurrentStringHashSet setCharactersToOpen = Interlocked.Exchange(ref _setCharactersToOpen, null);
            if (setCharactersToOpen == null || setCharactersToOpen.IsEmpty)
                return;

            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                CursorWait objCursorWait
                    = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    List<Character> lstCharacters = new List<Character>(setCharactersToOpen.Count);
                    using (ThreadSafeForm<LoadingBar> frmLoadingBar
                           = await Program.CreateAndShowProgressBarAsync(string.Empty,
                                                                         Character.NumLoadingSections
                                                                         * setCharactersToOpen.Count, token)
                                          .ConfigureAwait(false))
                    {
                        List<Task<Character>> tskCharacterLoads = new List<Task<Character>>(setCharactersToOpen.Count);
                        while (setCharactersToOpen.TryTake(out string strFile))
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            tskCharacterLoads.Add(Task.Run(
                                                      () => Program.LoadCharacterAsync(
                                                          strFile, frmLoadingBar: frmLoadingBar.MyForm,
                                                          token: token), token));
                        }
                        Character[] aobjCharacters = await Task.WhenAll(tskCharacterLoads).ConfigureAwait(false);
                        lstCharacters.AddRange(aobjCharacters);
                    }

                    await OpenCharacterList(lstCharacters, token: token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        /// <summary>
        /// Populate the MRU items.
        /// </summary>
        private async void PopulateMruToolstripMenu(object sender, TextEventArgs e)
        {
            try
            {
                await DoPopulateMruToolstripMenu(e?.Text, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoPopulateMruToolstripMenu(string strText = "", CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                await menuStrip.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    await menuStrip.DoThreadSafeAsync(() => mnuFileMRUSeparator.Visible
                                                          = GlobalSettings.FavoriteCharacters.Count > 0
                                                            || GlobalSettings.MostRecentlyUsedCharacters
                                                                             .Count > 0, token).ConfigureAwait(false);

                    if (strText != "mru")
                    {
                        for (int i = 0; i < GlobalSettings.MaxMruSize; ++i)
                        {
                            DpiFriendlyToolStripMenuItem objItem;
                            switch (i)
                            {
                                case 0:
                                    objItem = mnuStickyMRU0;
                                    break;

                                case 1:
                                    objItem = mnuStickyMRU1;
                                    break;

                                case 2:
                                    objItem = mnuStickyMRU2;
                                    break;

                                case 3:
                                    objItem = mnuStickyMRU3;
                                    break;

                                case 4:
                                    objItem = mnuStickyMRU4;
                                    break;

                                case 5:
                                    objItem = mnuStickyMRU5;
                                    break;

                                case 6:
                                    objItem = mnuStickyMRU6;
                                    break;

                                case 7:
                                    objItem = mnuStickyMRU7;
                                    break;

                                case 8:
                                    objItem = mnuStickyMRU8;
                                    break;

                                case 9:
                                    objItem = mnuStickyMRU9;
                                    break;

                                default:
                                    continue;
                            }

                            if (i < GlobalSettings.FavoriteCharacters.Count)
                            {
                                int i1 = i;
                                await menuStrip.DoThreadSafeAsync(() =>
                                {
                                    objItem.Text = GlobalSettings.FavoriteCharacters[i1];
                                    objItem.Tag = GlobalSettings.FavoriteCharacters[i1];
                                    objItem.Visible = true;
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await menuStrip.DoThreadSafeAsync(() => objItem.Visible = false, token).ConfigureAwait(false);
                            }
                        }
                    }

                    await menuStrip.DoThreadSafeAsync(() =>
                    {
                        mnuMRU0.Visible = false;
                        mnuMRU1.Visible = false;
                        mnuMRU2.Visible = false;
                        mnuMRU3.Visible = false;
                        mnuMRU4.Visible = false;
                        mnuMRU5.Visible = false;
                        mnuMRU6.Visible = false;
                        mnuMRU7.Visible = false;
                        mnuMRU8.Visible = false;
                        mnuMRU9.Visible = false;
                    }, token).ConfigureAwait(false);

                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                    int i2 = 0;
                    for (int i = 0; i < GlobalSettings.MaxMruSize; ++i)
                    {
                        if (i2 >= GlobalSettings.MostRecentlyUsedCharacters.Count ||
                            i >= GlobalSettings.MostRecentlyUsedCharacters.Count)
                            continue;
                        string strFile = GlobalSettings.MostRecentlyUsedCharacters[i];
                        if (await GlobalSettings.FavoriteCharacters.ContainsAsync(strFile, token).ConfigureAwait(false))
                            continue;
                        DpiFriendlyToolStripMenuItem objItem;
                        switch (i2)
                        {
                            case 0:
                                objItem = mnuMRU0;
                                break;

                            case 1:
                                objItem = mnuMRU1;
                                break;

                            case 2:
                                objItem = mnuMRU2;
                                break;

                            case 3:
                                objItem = mnuMRU3;
                                break;

                            case 4:
                                objItem = mnuMRU4;
                                break;

                            case 5:
                                objItem = mnuMRU5;
                                break;

                            case 6:
                                objItem = mnuMRU6;
                                break;

                            case 7:
                                objItem = mnuMRU7;
                                break;

                            case 8:
                                objItem = mnuMRU8;
                                break;

                            case 9:
                                objItem = mnuMRU9;
                                break;

                            default:
                                continue;
                        }

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (i2 <= 9
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            && i2 >= 0)
                        {
                            string strNumAsString = (i2 + 1).ToString(GlobalSettings.CultureInfo);
                            await menuStrip.DoThreadSafeAsync(
                                () => objItem.Text = strNumAsString.Insert(strNumAsString.Length - 1, "&") + strSpace
                                    + strFile, token).ConfigureAwait(false);
                        }
                        else
                        {
                            string strNumAsString = (i2 + 1).ToString(GlobalSettings.CultureInfo);
                            await menuStrip.DoThreadSafeAsync(
                                () => objItem.Text = strNumAsString + strSpace + strFile,
                                token).ConfigureAwait(false);
                        }

                        await menuStrip.DoThreadSafeAsync(() =>
                        {
                            objItem.Tag = strFile;
                            objItem.Visible = true;
                        }, token).ConfigureAwait(false);
                        ++i2;
                    }
                }
                finally
                {
                    await menuStrip.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        public async Task OpenDiceRollerWithPool(Character objCharacter = null, int intDice = 0, CancellationToken token = default)
        {
            CancellationTokenSource objSource = null;
            if (token != _objGenericToken)
            {
                objSource = CancellationTokenSource.CreateLinkedTokenSource(token, _objGenericToken);
                token = objSource.Token;
            }

            try
            {
                token.ThrowIfCancellationRequested();
                if (GlobalSettings.SingleDiceRoller)
                {
                    DiceRoller frmDiceRoller = RollerWindow;
                    if (frmDiceRoller == null)
                    {
                        DiceRoller frmNewDiceRoller = await this.DoThreadSafeFuncAsync(() => new DiceRoller(objCharacter?.Qualities, intDice), token).ConfigureAwait(false);
                        try
                        {
                            frmDiceRoller = Interlocked.CompareExchange(ref _frmDiceRoller, frmNewDiceRoller, null);
                            if (frmDiceRoller == null)
                            {
                                frmNewDiceRoller.FormClosing += (o, args) =>
                                    Interlocked.CompareExchange(ref _frmDiceRoller, null, frmNewDiceRoller);
                                await frmNewDiceRoller.DoThreadSafeAsync(x => x.Show(), token).ConfigureAwait(false);
                                return;
                            }
                        }
                        finally
                        {
                            if (frmDiceRoller != null)
                                await frmNewDiceRoller.DoThreadSafeAsync(x => x.Close(), CancellationToken.None).ConfigureAwait(false);
                        }
                    }
                    await frmDiceRoller.DoThreadSafeAsync(x =>
                    {
                        x.Dice = intDice;
                        x.ProcessGremlins(objCharacter?.Qualities);
                        x.Activate();
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    DiceRoller frmRoller
                        = await this.DoThreadSafeFuncAsync(() => new DiceRoller(objCharacter?.Qualities, intDice),
                                                           token).ConfigureAwait(false);
                    try
                    {
                        await frmRoller.DoThreadSafeAsync(x => x.Show(), token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        if (token.IsCancellationRequested)
                            await frmRoller.DoThreadSafeAsync(x => x.Close(), CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
            }
            finally
            {
                objSource?.Dispose();
            }
        }

        private void mnuClearUnpinnedItems_Click(object sender, EventArgs e)
        {
            GlobalSettings.MostRecentlyUsedCharacters.Clear();
        }

        private async void mnuRestart_Click(object sender, EventArgs e)
        {
            try
            {
                await Utils.RestartApplication(string.Empty, "Message_Options_Restart", _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (_objGenericToken.IsCancellationRequested)
            {
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == NativeMethods.WM_SHOWME)
                ShowMe();
            else if (m.Msg == NativeMethods.WM_COPYDATA && _blnAbleToReceiveData)
            {
                try
                {
                    using (CursorWait.New(this, true))
                    {
                        // Extract the file name
                        NativeMethods.CopyDataStruct objReceivedData
                            = (NativeMethods.CopyDataStruct) Marshal.PtrToStructure(
                                m.LParam, typeof(NativeMethods.CopyDataStruct));
                        _objGenericToken.ThrowIfCancellationRequested();
                        if (objReceivedData.dwData == Program.CommandLineArgsDataTypeId)
                        {
                            string strParam = Marshal.PtrToStringUni(objReceivedData.lpData) ?? string.Empty;
                            string[] strArgs = strParam.Split("<>", StringSplitOptions.RemoveEmptyEntries);

                            _objGenericToken.ThrowIfCancellationRequested();
                            bool blnShowTest;
                            using (ProcessCommandLineArguments(strArgs, out blnShowTest,
                                       out HashSet<string> setFilesToLoad))
                            {
                                _objGenericToken.ThrowIfCancellationRequested();
                                if (Directory.Exists(Utils.GetAutosavesFolderPath))
                                {
                                    // Always process newest autosave if all MRUs are empty
                                    bool blnAnyAutosaveInMru = GlobalSettings.MostRecentlyUsedCharacters.Count == 0 &&
                                                               GlobalSettings.FavoriteCharacters.Count == 0;
                                    FileInfo objMostRecentAutosave = null;
                                    foreach (string strAutosave in Directory.EnumerateFiles(
                                                 Utils.GetAutosavesFolderPath,
                                                 "*.chum5", SearchOption.AllDirectories).Concat(
                                                 Directory.EnumerateFiles(
                                                     Utils.GetAutosavesFolderPath,
                                                     "*.chum5lz", SearchOption.AllDirectories)))
                                    {
                                        _objGenericToken.ThrowIfCancellationRequested();
                                        FileInfo objAutosave;
                                        try
                                        {
                                            objAutosave = new FileInfo(strAutosave);
                                        }
                                        catch (SecurityException)
                                        {
                                            continue;
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            continue;
                                        }

                                        _objGenericToken.ThrowIfCancellationRequested();
                                        if (objMostRecentAutosave == null || objAutosave.LastWriteTimeUtc >
                                            objMostRecentAutosave.LastWriteTimeUtc)
                                            objMostRecentAutosave = objAutosave;
                                        string strAutosaveName = Path.GetFileNameWithoutExtension(objAutosave.Name);
                                        if (GlobalSettings.MostRecentlyUsedCharacters.Any(
                                                x => Path.GetFileNameWithoutExtension(x) == strAutosaveName,
                                                _objGenericToken) ||
                                            GlobalSettings.FavoriteCharacters.Any(
                                                x => Path.GetFileNameWithoutExtension(x) == strAutosaveName,
                                                _objGenericToken))
                                            blnAnyAutosaveInMru = true;
                                    }

                                    _objGenericToken.ThrowIfCancellationRequested();
                                    // Might have had a crash for an unsaved character, so prompt if we want to load them
                                    if (objMostRecentAutosave != null
                                        && blnAnyAutosaveInMru
                                        && !setFilesToLoad.Contains(objMostRecentAutosave.FullName))
                                    {
                                        _objGenericToken.ThrowIfCancellationRequested();
                                        string strAutosaveName
                                            = Path.GetFileNameWithoutExtension(objMostRecentAutosave.Name);
                                        if (GlobalSettings.MostRecentlyUsedCharacters.All(
                                                x => Path.GetFileNameWithoutExtension(x) != strAutosaveName,
                                                _objGenericToken)
                                            && GlobalSettings.FavoriteCharacters.All(
                                                x => Path.GetFileNameWithoutExtension(x) != strAutosaveName,
                                                _objGenericToken)
                                            && Program.ShowScrollableMessageBox(string.Format(
                                                    GlobalSettings.CultureInfo,
                                                    LanguageManager.GetString(
                                                        "Message_PossibleCrashAutosaveFound", token: _objGenericToken),
                                                    objMostRecentAutosave.Name,
                                                    objMostRecentAutosave.LastWriteTimeUtc
                                                        .ToLocalTime()),
                                                LanguageManager.GetString(
                                                    "MessageTitle_AutosaveFound", token: _objGenericToken),
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                            == DialogResult.Yes)
                                        {
                                            _objGenericToken.ThrowIfCancellationRequested();
                                            setFilesToLoad.Add(objMostRecentAutosave.FullName);
                                        }
                                    }
                                }

                                _objGenericToken.ThrowIfCancellationRequested();
                                if (setFilesToLoad.Count > 0)
                                {
                                    ConcurrentStringHashSet setNewCharactersToOpen = new ConcurrentStringHashSet();
                                    ConcurrentStringHashSet setCharactersToOpen
                                        = Interlocked.CompareExchange(
                                            ref _setCharactersToOpen, setNewCharactersToOpen, null);
                                    if (setCharactersToOpen != null)
                                        setNewCharactersToOpen = setCharactersToOpen;
                                    foreach (string strFile in setFilesToLoad)
                                    {
                                        _objGenericToken.ThrowIfCancellationRequested();
                                        setNewCharactersToOpen.TryAdd(strFile);
                                    }

                                    _tmrCharactersToOpenCheck.Start();
                                }
                            }

                            if (blnShowTest)
                            {
                                TestDataEntries frmTestData = new TestDataEntries();
                                frmTestData.Show();
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }

            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            // get our current "TopMost" value (ours will always be false though)
            bool blnOldTopMost = TopMost;
            try
            {
                // make our form jump to the top of everything
                TopMost = true;
            }
            finally
            {
                // set it back to whatever it was
                TopMost = blnOldTopMost;
            }
        }

        private static FetchSafelyFromPool<HashSet<string>> ProcessCommandLineArguments(IReadOnlyCollection<string> strArgs, out bool blnShowTest, out HashSet<string> setFilesToLoad, CustomActivity opLoadActivity = null)
        {
            blnShowTest = false;
            FetchSafelyFromPool<HashSet<string>> objReturn = new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool, out setFilesToLoad);
            if (strArgs.Count == 0)
                return objReturn;
            try
            {
                foreach (string strArg in strArgs)
                {
                    if (strArg.EndsWith(Path.GetFileName(Application.ExecutablePath), StringComparison.OrdinalIgnoreCase))
                        continue;
                    switch (strArg)
                    {
                        case "/test":
                            blnShowTest = true;
                            break;

                        case "/help":
                        case "?":
                        case "/?":
                            string msg = "Commandline parameters are either " +
                                             Environment.NewLine + "\t/test" + Environment.NewLine +
                                             "\t/help" + Environment.NewLine +
                                             "\t(filename to open)" +
                                             Environment.NewLine +
                                             "\t/plugin:pluginname (like \"SINners\") to trigger (with additional parameters following the symbol \":\")" +
                                             Environment.NewLine;
                            Console.WriteLine(msg);
                            break;
                        default:
                            if (strArg.Contains("/plugin"))
                            {
                                Log.Info(
                                    "Encountered command line argument, that should already have been handled in one of the plugins: " +
                                    strArg);
                            }
                            else if (!strArg.StartsWith('/'))
                            {
                                if (!File.Exists(strArg))
                                {
                                    throw new ArgumentException(
                                        "Chummer started with unknown command line arguments: " +
                                        strArgs.Aggregate((j, k) => j + ' ' + k));
                                }

                                string strExtension = Path.GetExtension(strArg);
                                if (!string.Equals(strExtension, ".chum5", StringComparison.OrdinalIgnoreCase)
                                    && !string.Equals(strExtension, ".chum5lz", StringComparison.OrdinalIgnoreCase))
                                    Utils.BreakIfDebug();
                                setFilesToLoad.Add(strArg);
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (opLoadActivity != null)
                {
                    opLoadActivity.SetSuccess(false);
                    ExceptionTelemetry ext = new ExceptionTelemetry(ex)
                    {
                        SeverityLevel = SeverityLevel.Warning
                    };
                    opLoadActivity.MyTelemetryClient.TrackException(ext);
                }
                Log.Warn(ex);
            }

            return objReturn;
        }

#endregion Methods

#region Application Properties

        /// <summary>
        /// The frmDiceRoller window being used by the application.
        /// </summary>
        public DiceRoller RollerWindow => _frmDiceRoller;

        public ThreadSafeObservableCollection<CharacterShared> OpenCharacterEditorForms => _lstOpenCharacterEditorForms;

        public ThreadSafeObservableCollection<CharacterSheetViewer> OpenCharacterSheetViewers =>
            _lstOpenCharacterSheetViewers;

        public ThreadSafeObservableCollection<ExportCharacter> OpenCharacterExportForms =>
            _lstOpenCharacterExportForms;

        public IEnumerable<IHasCharacterObjects> OpenFormsWithCharacters
        {
            get
            {
                // Weird structure with assignment to locals and null checks needed in case we dispose the list in the middle of an iteration
                ThreadSafeObservableCollection<CharacterShared> lstForms1 = _lstOpenCharacterEditorForms;
                if (lstForms1 != null)
                {
                    foreach (CharacterShared frmLoop in lstForms1)
                        yield return frmLoop;
                }

                ThreadSafeObservableCollection<CharacterSheetViewer> lstForms2 = _lstOpenCharacterSheetViewers;
                if (lstForms2 != null)
                {
                    foreach (CharacterSheetViewer frmLoop in _lstOpenCharacterSheetViewers)
                        yield return frmLoop;
                }

                ThreadSafeObservableCollection<ExportCharacter> lstForms3 = _lstOpenCharacterExportForms;
                if (lstForms3 != null)
                {
                    foreach (ExportCharacter frmLoop in _lstOpenCharacterExportForms)
                        yield return frmLoop;
                }
            }
        }

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }

#endregion Application Properties
    }
}
